using PncNext.Domain.Interfaces;
using PncNext.Domain.Models;
using System.Collections.Concurrent;

namespace PncNext.Infrastructure.Motion.Channels
{
    /// <summary>
    /// 공통 비동기 통신 로직 및 포트 관리를 담당하는 채널 추상 베이스 클래스.
    /// </summary>
    public abstract class MotionChannelBase : IDisposable
    {
        protected readonly ICommPort _port;
        protected readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> _pendingRequests = new();
        private CancellationTokenSource? _loopCts;
        private Task? _receiveLoopTask;

        public event EventHandler<byte[]>? MessageReceived;

        protected MotionChannelBase(ICommPort port)
        {
            _port = port;
        }

        public bool IsOpen => _port.IsOpen;
        public bool IsFaulted { get; protected set; }

        public virtual async Task OpenAsync()
        {
            if (IsOpen && !IsFaulted) return;

            if (IsFaulted || _receiveLoopTask != null)
            {
                await CloseAsync();
            }

            IsFaulted = false;
            await _port.OpenAsync();
            StartReceiveLoop();
        }

        public virtual async Task CloseAsync()
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

                    string responseKey = ExtractCommandKey(data);
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

        protected abstract string ExtractCommandKey(byte[] response);

        protected void SetFault()
        {
            IsFaulted = true;
            _ = CloseAsync();
        }

        protected async Task<byte[]> SendAndReceiveAsync(MotionCommandInfo cmdInfo)
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

        protected byte[] SendAndReceiveSync(MotionCommandInfo cmdInfo)
        {
            return SendAndReceiveAsync(cmdInfo).GetAwaiter().GetResult();
        }

        protected void CheckState()
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
