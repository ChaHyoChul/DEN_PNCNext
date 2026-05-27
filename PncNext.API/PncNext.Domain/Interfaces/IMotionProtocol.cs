using PncNext.Domain.Models;

namespace PncNext.Domain.Interfaces
{
    public interface IMotionProtocol
    {
        /// <summary>
        /// 장비 정지 명령어를 생성합니다.
        /// </summary>
        MotionCommandInfo EncodeStop(int mode);

        MotionCommandInfo EncodeHalt();

        MotionCommandInfo EncodeErrorReset();

        MotionCommandInfo EncodeInitController();
        /// <summary>
        /// 장비 원점 복귀 명령어를 생성합니다.
        /// </summary>
        MotionCommandInfo EncodeHome();
        
        /// <summary>
        /// 상태 조회를 위한 명령어 정보를 생성합니다.
        /// </summary>
        MotionCommandInfo EncodeStatusRequest();

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
