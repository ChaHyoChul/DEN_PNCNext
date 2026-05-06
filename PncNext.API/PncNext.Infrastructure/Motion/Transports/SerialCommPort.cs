using PncNext.Domain.Interfaces;
using System.IO.Ports;

namespace PncNext.Infrastructure.Motion.Transports
{
    public class SerialCommPort : ICommPort, IDisposable
    {
        private readonly string _portName;
        private readonly int _baudRate;
        private SerialPort? _serialPort;

        public SerialCommPort(string portName, int baudRate)
        {
            _portName = portName;
            _baudRate = baudRate;
        }

        public bool IsOpen => _serialPort?.IsOpen ?? false;

        public async Task OpenAsync()
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

        public async Task CloseAsync()
        {
            if (IsOpen)
            {
                _serialPort?.Close();
            }
            _serialPort?.Dispose();
            _serialPort = null;
            await Task.CompletedTask;
        }

        public async Task SendAsync(byte[] data)
        {
            if (!IsOpen) throw new InvalidOperationException("Serial port is not open");
            _serialPort!.Write(data, 0, data.Length);
            await Task.CompletedTask;
        }

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
        }
    }
}
