using PncNext.Domain.Interfaces;
using System.IO.Ports;

namespace PncNext.Infrastructure.Motion.Transports
{
    /// <summary>
    /// Serial 포트 통신 클래스.
    /// 송수신 자원을 분리하여 데드락을 방지합니다.
    /// </summary>
    public class SerialCommPort : ICommPort, IDisposable
    {
        private readonly string _portName;
        private readonly int _baudRate;
        private SerialPort? _serialPort;
        
        // 송신 및 연결 제어 전용 락
        private readonly SemaphoreSlim _syncLock = new SemaphoreSlim(1, 1);

        public SerialCommPort(string portName, int baudRate)
        {
            _portName = portName;
            _baudRate = baudRate;
        }

        public bool IsOpen => _serialPort?.IsOpen ?? false;

        public async Task OpenAsync()
        {
            await _syncLock.WaitAsync();
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
                _syncLock.Release();
            }
        }

        public async Task CloseAsync()
        {
            await _syncLock.WaitAsync();
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
                _syncLock.Release();
            }
        }

        public async Task SendAsync(byte[] data)
        {
            await _syncLock.WaitAsync();
            try
            {
                if (!IsOpen) throw new InvalidOperationException("Serial port is not open");
                _serialPort!.Write(data, 0, data.Length);
                await Task.CompletedTask;
            }
            finally
            {
                _syncLock.Release();
            }
        }

        /// <summary>
        /// 시리얼 포트로부터 데이터를 수신합니다.
        /// (데드락 방지를 위해 Lock을 사용하지 않습니다)
        /// </summary>
        public async Task<byte[]> ReceiveAsync()
        {
            if (!IsOpen) throw new InvalidOperationException("Serial port is not open");
            
            byte[] buffer = new byte[1024];
            int read = await _serialPort!.BaseStream.ReadAsync(buffer, 0, buffer.Length);
            
            var result = new byte[read];
            Array.Copy(buffer, result, read);
            return result;
        }

        public void Dispose()
        {
            _serialPort?.Dispose();
            _syncLock.Dispose();
        }
    }
}
