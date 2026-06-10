using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PncNext.Domain.Interfaces;

namespace PncNext.Infrastructure.Services
{
    /// <summary>
    /// ISignalRService의 임시(Stub) 구현체. 실제 SignalR Hub 로직 연결 전까지 로그만 출력합니다.
    /// </summary>
    public class SignalRServiceStub : ISignalRService
    {
        private readonly ILogger<SignalRServiceStub> _logger;

        public SignalRServiceStub(ILogger<SignalRServiceStub> logger)
        {
            _logger = logger;
        }

        public Task BroadcastAsync(string eventName, object payload)
        {
            _logger.LogInformation($"[SignalR Broadcast] Event: {eventName}, Payload: {System.Text.Json.JsonSerializer.Serialize(payload)}");
            return Task.CompletedTask;
        }
    }
}
