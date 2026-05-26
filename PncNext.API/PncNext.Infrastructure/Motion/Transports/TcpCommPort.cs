using PncNext.Domain.Interfaces;
using System.Net.Sockets;
using System.Text;

namespace PncNext.Infrastructure.Motion.Transports
{
    /// <summary>
    /// PA 제어기 사양(PAAsyncComm.md)을 반영하여 개선된 TCP 통신 클래스.
    /// 송수신 자원을 분리하여 데드락을 방지합니다.
    /// </summary>
    public class TcpCommPort : ICommPort
    {
        private readonly string _ip;
        private readonly int _port;
        private TcpClient? _client;
        private readonly StringBuilder _receiveBuffer = new StringBuilder();
        
        // 송신 및 연결 제어 전용 락 (수신부와 분리)
        private readonly SemaphoreSlim _syncLock = new SemaphoreSlim(1, 1);

        public TcpCommPort(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        public bool IsOpen => _client?.Connected ?? false;

        public async Task OpenAsync()
        {
            await _syncLock.WaitAsync();
            try
            {
                if (IsOpen) return;
                _client = new TcpClient();
                await _client.ConnectAsync(_ip, _port);
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
                _client?.Close();
                _client = null;
                _receiveBuffer.Clear();
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
                if (!IsOpen) throw new InvalidOperationException("Port is not open");
                var stream = _client!.GetStream();
                await stream.WriteAsync(data, 0, data.Length);
                await stream.FlushAsync();
            }
            finally
            {
                _syncLock.Release();
            }
        }

        /// <summary>
        /// 데이터를 읽어 CRLF(\r\n) 단위로 분리하여 반환합니다.
        /// (수신부는 단일 루프에서 호출되므로 데드락 방지를 위해 Lock을 사용하지 않습니다)
        /// </summary>
        public async Task<byte[]> ReceiveAsync()
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
                // 소켓 읽기 작업은 송신부 락과 무관하게 병렬로 실행 가능
                int read = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (read == 0)
                {
                    await CloseAsync();
                    throw new Exception("Disconnected from server");
                }

                _receiveBuffer.Append(Encoding.ASCII.GetString(buffer, 0, read));
                
                line = TryExtractLine();
                if (line != null) return Encoding.ASCII.GetBytes(line);
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
