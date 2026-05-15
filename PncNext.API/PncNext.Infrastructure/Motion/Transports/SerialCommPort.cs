using PncNext.Domain.Interfaces;
using System.IO.Ports;

namespace PncNext.Infrastructure.Motion.Transports
{
    /// <summary>
    /// Serial 포트 통신 클래스.
    /// SemaphoreSlim을 통해 다중 서비스 간의 자원 경합을 방지합니다 (Thread-safe).
    /// </summary>
    public class SerialCommPort : ICommPort, IDisposable
    {
        private readonly string _portName;
        private readonly int _baudRate;
        private SerialPort? _serialPort;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public SerialCommPort(string portName, int baudRate)
        {
            _portName = portName;
            _baudRate = baudRate;
        }

        public bool IsOpen => _serialPort?.IsOpen ?? false;

        public async Task OpenAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (IsOpen) return;

                _serialPort = new SerialPort(_portName, _baudRate)
                {
                    ReadTimeout = 500,
                    WriteTimeout = 500
                };

                _serialPort.Open();
                await Task.CompletedTask;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task CloseAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (IsOpen)
                {
                    _serialPort?.Close();
                }
                _serialPort?.Dispose();
                _serialPort = null;
                await Task.CompletedTask;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SendAsync(byte[] data)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!IsOpen) throw new InvalidOperationException("Serial port is not open");
                _serialPort!.Write(data, 0, data.Length);
                await Task.CompletedTask;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<byte[]> ReceiveAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!IsOpen) throw new InvalidOperationException("Serial port is not open");
                
                byte[] buffer = new byte[1024];
                int read = await _serialPort!.BaseStream.ReadAsync(buffer, 0, buffer.Length);
                
                var result = new byte[read];
                Array.Copy(buffer, result, read);
                return result;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            _serialPort?.Dispose();
            _semaphore.Dispose();
        }
    }
}
