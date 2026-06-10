using System.Threading.Tasks;

namespace PncNext.Domain.Interfaces
{
    /// <summary>
    /// 멀티 클라이언트 간의 상태 동기화를 위한 SignalR 실시간 통신 서비스 인터페이스
    /// </summary>
    public interface ISignalRService
    {
        /// <summary>
        /// 모든 연결된 클라이언트에게 지정된 이벤트를 브로드캐스트합니다.
        /// </summary>
        /// <param name="eventName">클라이언트가 수신할 이벤트 이름 (예: "DiskCreatedAndHealed")</param>
        /// <param name="payload">이벤트와 함께 전송할 데이터 객체</param>
        Task BroadcastAsync(string eventName, object payload);
    }
}
