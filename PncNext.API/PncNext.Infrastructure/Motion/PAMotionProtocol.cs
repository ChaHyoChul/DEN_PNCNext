using PncNext.Domain.Interfaces;
using System.Text;

namespace PncNext.Infrastructure.Motion
{
    /// <summary>
    /// PncNext 전용 모션 프로토콜 구현 클래스 (PAMotionProtocol)
    /// </summary>
    public class PAMotionProtocol : IMotionProtocol
    {
        // 덴탈 장비 프로토콜 특성을 반영한 시작/종료 문자 (예시)
        private const string STX = "\x02";
        private const string ETX = "\x03";

        public byte[] EncodeMove(double x, double y, double z, double a, double b)
        {
            // 예시: <STX>MOV:X10.5,Y20.0,Z-5.0,A0.0,B0.0<ETX>
            string command = $"{STX}MOV:X{x:F3},Y{y:F3},Z{z:F3},A{a:F3},B{b:F3}{ETX}";
            return Encoding.ASCII.GetBytes(command);
        }

        public byte[] EncodeStop()
        {
            // 예시: <STX>STP<ETX>
            string command = $"{STX}STP{ETX}";
            return Encoding.ASCII.GetBytes(command);
        }

        public byte[] EncodeStatusRequest()
        {
            // 예시: <STX>STS?<ETX>
            string command = $"{STX}STS?{ETX}";
            return Encoding.ASCII.GetBytes(command);
        }

        public MotionStatus DecodeStatus(byte[] response)
        {
            if (response == null || response.Length == 0)
                return MotionStatus.Error;

            string resStr = Encoding.ASCII.GetString(response);

            // 프로토콜 응답 해석 로직 (예시)
            if (resStr.Contains("RUN")) return MotionStatus.Running;
            if (resStr.Contains("IDL")) return MotionStatus.Idle;
            if (resStr.Contains("ERR")) return MotionStatus.Error;
            if (resStr.Contains("STP")) return MotionStatus.Stopped;

            return MotionStatus.Idle;
        }
    }
}
