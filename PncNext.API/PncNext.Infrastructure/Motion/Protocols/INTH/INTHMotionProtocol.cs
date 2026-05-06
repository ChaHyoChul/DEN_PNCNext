using PncNext.Domain.Interfaces;
using System.Text;

namespace PncNext.Infrastructure.Motion.Protocols.INTH
{
    /// <summary>
    /// INTH 모션 제어기 전용 프로토콜 구현 클래스
    /// </summary>
    public class INTHMotionProtocol : IMotionProtocol
    {
        // INTH 장비 프로토콜 특성을 반영한 구분자 (예시)
        private const string HEADER = "@INTH";
        private const string FOOTER = "#";

        public byte[] EncodeMove(double x, double y, double z, double a, double b)
        {
            // 예시: @INTH|MOV|10.500|20.000|-5.000|0.000|0.000#
            string command = $"{HEADER}|MOV|{x:F3}|{y:F3}|{z:F3}|{a:F3}|{b:F3}{FOOTER}";
            return Encoding.ASCII.GetBytes(command);
        }

        public byte[] EncodeStop()
        {
            // 예시: @INTH|STP#
            string command = $"{HEADER}|STP{FOOTER}";
            return Encoding.ASCII.GetBytes(command);
        }

        public byte[] EncodeStatusRequest()
        {
            // 예시: @INTH|GET_STS#
            string command = $"{HEADER}|GET_STS{FOOTER}";
            return Encoding.ASCII.GetBytes(command);
        }

        public MotionStatus DecodeStatus(byte[] response)
        {
            if (response == null || response.Length == 0)
                return MotionStatus.Error;

            string resStr = Encoding.ASCII.GetString(response);

            // INTH 프로토콜 응답 해석 로직 (예시)
            if (resStr.Contains("RUNNING")) return MotionStatus.Running;
            if (resStr.Contains("IDLE")) return MotionStatus.Idle;
            if (resStr.Contains("ALARM")) return MotionStatus.Error;
            if (resStr.Contains("PAUSE")) return MotionStatus.Stopped;

            return MotionStatus.Idle;
        }
    }
}
