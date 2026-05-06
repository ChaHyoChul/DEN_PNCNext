using PncNext.Domain.Interfaces;
using System.Net.Sockets;

namespace PncNext.Infrastructure.Motion.Transports
{
    public class TcpCommPort : ICommPort
    {
        private readonly string _ip;
        private readonly int _port;
        private TcpClient? _client;

        public TcpCommPort(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        public bool IsOpen => _client?.Connected ?? false;

        public async Task OpenAsync()
        {
            if (IsOpen) return;
            _client = new TcpClient();
            await _client.ConnectAsync(_ip, _port);
        }

        public async Task CloseAsync()
        {
            _client?.Close();
            _client = null;
            await Task.CompletedTask;
        }

        public async Task SendAsync(byte[] data)
        {
            if (!IsOpen) throw new InvalidOperationException("Port is not open");
            var stream = _client!.GetStream();
            await stream.WriteAsync(data, 0, data.Length);
        }

        public async Task<byte[]> ReceiveAsync()
        {
            if (!IsOpen) throw new InvalidOperationException("Port is not open");
            var stream = _client!.GetStream();
            byte[] buffer = new byte[1024];
            int read = await stream.ReadAsync(buffer, 0, buffer.Length);
            var result = new byte[read];
            Array.Copy(buffer, result, read);
            return result;
        }
    }
}
