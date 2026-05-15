using PncNext.Domain.Interfaces;
using System.Net.Sockets;
using System.Text;

namespace PncNext.Infrastructure.Motion.Transports
{
    /// <summary>
    /// PA 제어기 사양(PAAsyncComm.md)을 반영하여 개선된 TCP 통신 클래스.
    /// SemaphoreSlim을 통해 다중 서비스 간의 자원 경합을 방지합니다 (Thread-safe).
    /// </summary>
    public class TcpCommPort : ICommPort
    {
        private readonly string _ip;
        private readonly int _port;
        private TcpClient? _client;
        private readonly StringBuilder _receiveBuffer = new StringBuilder();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public TcpCommPort(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        public bool IsOpen => _client?.Connected ?? false;

        public async Task OpenAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (IsOpen) return;
                _client = new TcpClient();
                await _client.ConnectAsync(_ip, _port);
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
                _client?.Close();
                _client = null;
                _receiveBuffer.Clear();
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
                if (!IsOpen) throw new InvalidOperationException("Port is not open");
                var stream = _client!.GetStream();
                await stream.WriteAsync(data, 0, data.Length);
                await stream.FlushAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// 데이터를 읽어 CRLF(\r\n) 단위로 분리하여 반환합니다.
        /// </summary>
        public async Task<byte[]> ReceiveAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!IsOpen) throw new InvalidOperationException("Port is not open");
                
                var stream = _client!.GetStream();
                byte[] buffer = new byte[2048];

                // 이미 버퍼에 완성된 라인이 있는지 확인
                string? line = TryExtractLine();
                if (line != null) return Encoding.ASCII.GetBytes(line);

                // 데이터가 올 때까지 읽기
                while (true)
                {
                    int read = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (read == 0) throw new Exception("Disconnected from server");

                    _receiveBuffer.Append(Encoding.ASCII.GetString(buffer, 0, read));
                    
                    line = TryExtractLine();
                    if (line != null) return Encoding.ASCII.GetBytes(line);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private string? TryExtractLine()
        {
            string content = _receiveBuffer.ToString();
            int index = content.IndexOf("\r\n");
            if (index >= 0)
            {
                string line = content.Substring(0, index);
                _receiveBuffer.Remove(0, index + 2);
                return line;
            }
            return null;
        }
    }
}
