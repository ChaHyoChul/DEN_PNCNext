using PncNext.Domain.Models;

namespace PncNext.Domain.Interfaces
{
    public interface IMotionProtocol
    {
        // G-코드 명령 관련 (현재 정리 중이므로 MotionCommandInfo 반환으로 인터페이스 통일)
        MotionCommandInfo EncodeMove(double x, double y, double z, double a, double b);
        MotionCommandInfo EncodeStop();
        
        /// <summary>
        /// 상태 조회를 위한 명령어 정보를 생성합니다.
        /// </summary>
        MotionCommandInfo EncodeStatusRequest();
        
        MotionStatus DecodeStatus(byte[] response);

        /// <summary>
        /// 커스텀 명령어를 해당 프로토콜 형식에 맞는 정보 객체로 생성합니다.
        /// </summary>
        MotionCommandInfo EncodeCustom(string command, params object[] args);

        /// <summary>
        /// 수신된 응답 데이터에서 매칭할 Command Key를 추출합니다.
        /// </summary>
        string ExtractCommandKey(byte[] response);
    }
}
