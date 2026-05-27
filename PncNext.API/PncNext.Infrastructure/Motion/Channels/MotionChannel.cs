using PncNext.Domain.Interfaces;
using PncNext.Domain.Models;
using System.Collections.Concurrent;

namespace PncNext.Infrastructure.Motion.Channels
{
    /// <summary>
    /// ICommPort(전송)와 IMotionProtocol(인코딩)을 결합한 IMotionChannel 구현체.
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
            if (IsOpen && !IsFaulted)
            {
                return;
            }

            if (IsFaulted || _receiveLoopTask != null)
            {
                await CloseAsync();
            }

            IsFaulted = false;

            await _port.OpenAsync();
            StartReceiveLoop();
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
            _receiveLoopTask = null;
        }

        private async Task ReceiveLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested && IsOpen)
                {
                    byte[] data = await _port.ReceiveAsync();
                    if (data == null || data.Length == 0) continue;

                    MessageReceived?.Invoke(this, data);

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

        public async Task StopAsync(int mode)
        {
            CheckState();
            var cmdInfoStop = _protocol.EncodeStop(mode);
            await SendAndReceiveAsync(cmdInfoStop);
            var cmdInfoHalt = _protocol.EncodeHalt();
            await SendAndReceiveAsync(cmdInfoHalt);
        }

        public async Task ErrorResetAsync()
        {
            var cmdInfo = _protocol.EncodeErrorReset();
            await SendAndReceiveAsync(cmdInfo);
        }

        public async Task InitControllerAsync()
        {
            var cmdInfo = _protocol.EncodeInitController();
            await SendAndReceiveAsync(cmdInfo);
        }

        public async Task HomeAsync()
        {
            CheckState();
            var cmdInfo = _protocol.EncodeHome();
            // 원점 복귀는 장시간 대기하므로 프로토콜에서 정의된 긴 타임아웃이 적용됨
            await SendAndReceiveAsync(cmdInfo);
        }

        public async Task<MotionStatus> GetStatusAsync()
        {
            var response = await ReadFullStatusAsync();
            return MotionStatus.Ready;
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

        public async Task<byte[]> ReceiveRawAsync()
        {
            return await ReadFullStatusAsync();
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
