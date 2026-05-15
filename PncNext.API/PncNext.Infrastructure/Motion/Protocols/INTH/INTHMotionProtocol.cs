using PncNext.Domain.Interfaces;
using System.Text;

namespace PncNext.Infrastructure.Motion.Protocols.INTH
{
    /// <summary>
    /// INTH 모션 제어기 전용 프로토콜 구현 클래스
    /// </summary>
    public class INTHMotionProtocol : IMotionProtocol
    {
        private const string HEADER = "@INTH";
        private const string FOOTER = "#";

        public byte[] EncodeMove(double x, double y, double z, double a, double b)
        {
            string command = $"{HEADER}|MOV|{x:F3}|{y:F3}|{z:F3}|{a:F3}|{b:F3}{FOOTER}";
            return Encoding.ASCII.GetBytes(command);
        }

        public byte[] EncodeStop()
        {
            string command = $"{HEADER}|STP{FOOTER}";
            return Encoding.ASCII.GetBytes(command);
        }

        public byte[] EncodeStatusRequest()
        {
            string command = $"{HEADER}|GET_STS{FOOTER}";
            return Encoding.ASCII.GetBytes(command);
        }

        public MotionStatus DecodeStatus(byte[] response)
        {
            if (response == null || response.Length == 0)
                return MotionStatus.Error;

            string resStr = Encoding.ASCII.GetString(response);

            if (resStr.Contains("RUNNING")) return MotionStatus.Running;
            if (resStr.Contains("IDLE")) return MotionStatus.Idle;
            if (resStr.Contains("ALARM")) return MotionStatus.Error;
            if (resStr.Contains("PAUSE")) return MotionStatus.Stopped;

            return MotionStatus.Idle;
        }

        public byte[] EncodeCustom(string command, params object[] args)
        {
            string cmd = args.Length > 0 ? string.Format(command, args) : command;
            return Encoding.ASCII.GetBytes($"{HEADER}|{cmd}{FOOTER}");
        }
    }
}
