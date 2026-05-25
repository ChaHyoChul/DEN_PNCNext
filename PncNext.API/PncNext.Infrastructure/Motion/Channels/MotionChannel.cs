using PncNext.Domain.Interfaces;
using PncNext.Domain.Models;
using System.Collections.Concurrent;

namespace PncNext.Infrastructure.Motion.Channels
{
    /// <summary>
    /// ICommPort(전송)와 IMotionProtocol(인코딩)을 결합한 IMotionChannel 구현체.
    /// 비동기 수신 루프 및 타임아웃 에러 처리 기능을 포함합니다.
    /// </summary>
    public class MotionChannel : IMotionChannel, IDisposable
    {
        private readonly ICommPort _port;
        private readonly IMotionProtocol _protocol;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> _pendingRequests = new();
        private CancellationTokenSource? _loopCts;
        private Task? _receiveLoopTask;

        public event EventHandler<byte[]>? MessageReceived;

        public MotionChannel(ICommPort port, IMotionProtocol protocol)
        {
            _port = port;
            _protocol = protocol;
        }

        public bool IsOpen => _port.IsOpen;
        public bool IsFaulted { get; private set; }
        public IMotionProtocol Protocol => _protocol;

        public async Task OpenAsync()
        {
            if (IsFaulted) throw new InvalidOperationException("Channel is in faulted state. Call ResetFault() first.");
            
            if (!IsOpen)
            {
                await _port.OpenAsync();
                StartReceiveLoop();
            }
        }

        public async Task CloseAsync()
        {
            StopReceiveLoop();
            await _port.CloseAsync();
        }

        public void ResetFault()
        {
            IsFaulted = false;
        }

        private void StartReceiveLoop()
        {
            _loopCts = new CancellationTokenSource();
            _receiveLoopTask = Task.Run(() => ReceiveLoopAsync(_loopCts.Token));
        }

        private void StopReceiveLoop()
        {
            _loopCts?.Cancel();
            _loopCts?.Dispose();
            _loopCts = null;
            
            foreach (var tcs in _pendingRequests.Values)
            {
                tcs.TrySetCanceled();
            }
            _pendingRequests.Clear();
        }

        private async Task ReceiveLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested && IsOpen)
                {
                    byte[] data = await _port.ReceiveAsync();
                    if (data == null || data.Length == 0) continue;

                    // 1. 이벤트 발생 (상태 자동 업데이트용)
                    MessageReceived?.Invoke(this, data);

                    // 2. 요청-응답 매칭 (TCS 완료)
                    // 프로토콜에서 응답의 키를 추출하여 정확한 대기자에게 전달
                    string responseKey = _protocol.ExtractCommandKey(data);
                    if (!string.IsNullOrEmpty(responseKey) && _pendingRequests.TryRemove(responseKey, out var tcs))
                    {
                        tcs.TrySetResult(data);
                    }
                }
            }
            catch (Exception)
            {
                if (!ct.IsCancellationRequested)
                {
                    SetFault();
                }
            }
        }

        private void SetFault()
        {
            IsFaulted = true;
            _ = CloseAsync();
        }

        public async Task<MotionStatus> GetStatusAsync()
        {
            var response = await ReadFullStatusAsync();
            return _protocol.DecodeStatus(response);
        }

        public async Task<byte[]> ReadFullStatusAsync()
        {
            var cmdInfo = _protocol.EncodeStatusRequest();
            return await SendAndReceiveAsync(cmdInfo);
        }

        public async Task<byte[]> SendCustomCommandAsync(string command, params object[] args)
        {
            var cmdInfo = _protocol.EncodeCustom(command, args);
            return await SendAndReceiveAsync(cmdInfo);
        }

        private async Task<byte[]> SendAndReceiveAsync(MotionCommandInfo cmdInfo)
        {
            CheckState();
            if (!IsOpen) await OpenAsync();

            var tcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingRequests[cmdInfo.CommandKey] = tcs;

            try
            {
                await _port.SendAsync(cmdInfo.Payload);

                // 프로토콜이 지정한 타임아웃 시간 적용
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(cmdInfo.TimeoutMs));
                using (timeoutCts.Token.Register(() => tcs.TrySetException(new TimeoutException($"Command '{cmdInfo.CommandKey}' timed out after {cmdInfo.TimeoutMs}ms."))))
                {
                    return await tcs.Task;
                }
            }
            catch (TimeoutException)
            {
                SetFault();
                throw;
            }
            catch (Exception)
            {
                SetFault();
                throw;
            }
        }

        private void CheckState()
        {
            if (IsFaulted) throw new InvalidOperationException("Channel is in faulted state due to communication error or timeout.");
        }

        public void Dispose()
        {
            StopReceiveLoop();
            _port?.CloseAsync().Wait();
        }
    }
}
