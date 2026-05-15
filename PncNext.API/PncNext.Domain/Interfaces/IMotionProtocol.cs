namespace PncNext.Domain.Interfaces
{
    public interface IMotionProtocol
    {
        byte[] EncodeMove(double x, double y, double z, double a, double b);
        byte[] EncodeStop();
        byte[] EncodeStatusRequest();
        MotionStatus DecodeStatus(byte[] response);

        /// <summary>
        /// 커스텀 명령어를 해당 프로토콜 형식에 맞게 인코딩합니다.
        /// </summary>
        byte[] EncodeCustom(string command, params object[] args);
    }
}
