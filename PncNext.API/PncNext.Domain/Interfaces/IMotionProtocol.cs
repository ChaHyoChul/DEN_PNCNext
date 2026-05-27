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
        /// 장비 동작 모드 전환 명령어를 생성합니다.
        /// </summary>
        MotionCommandInfo EncodeMode(string mode);

        MotionCommandInfo EncodePause();

        MotionCommandInfo EncodeContinue();

        /// <summary>
        /// MDA(Manual Data Input) 명령어를 생성합니다. (G-Code 실행)
        /// </summary>
        /// <param name="gcode">실행할 G-Code 문자열</param>
        MotionCommandInfo EncodeMda(string gcode);

        /// <summary>
        /// 상대 위치 이동 명령어를 생성합니다. (입력된 축만 이동)
        /// </summary>
        MotionCommandInfo EncodeMoveIncremental(double? x, double? y, double? z, double? a, double? b);

        /// <summary>
        /// 절대 위치 이동 명령어를 생성합니다. (입력된 축만 이동)
        /// </summary>
        MotionCommandInfo EncodeMoveAbsolute(double? x, double? y, double? z, double? a, double? b);

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
