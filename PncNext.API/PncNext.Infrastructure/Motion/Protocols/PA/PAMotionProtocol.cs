using PncNext.Domain.Interfaces;
using System.Text;
using System.Globalization;

namespace PncNext.Infrastructure.Motion.Protocols.PA
{
    /// <summary>
    /// PA 모션 제어기 비동기 통신 규격(PAAsyncComm.md)을 반영한 프로토콜 구현 클래스
    /// </summary>
    public class PAMotionProtocol : IMotionProtocol
    {
        private const string TERMINATOR = "\r\n";

        public byte[] EncodeMove(double x, double y, double z, double a, double b)
        {
            string command = string.Format(CultureInfo.InvariantCulture, 
                "MOV {0:F3} {1:F3} {2:F3} {3:F3} {4:F3}{5}", 
                x, y, z, a, b, TERMINATOR);
            return Encoding.ASCII.GetBytes(command);
        }

        public byte[] EncodeStop()
        {
            return Encoding.ASCII.GetBytes($"RND_STOP{TERMINATOR}");
        }

        public byte[] EncodeStatusRequest()
        {
            return Encoding.ASCII.GetBytes($"RND_STATUS{TERMINATOR}");
        }

        public MotionStatus DecodeStatus(byte[] response)
        {
            if (response == null || response.Length == 0)
                return MotionStatus.Error;

            string resStr = Encoding.ASCII.GetString(response).Trim();

            if (resStr.Contains("PS:"))
            {
                var allValues = resStr.Replace("RND_STATUS", "").Trim().Split(new[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (allValues.Length >= 5)
                {
                    if (int.TryParse(allValues[allValues.Length - 1], out int runStatus))
                    {
                        return runStatus switch
                        {
                            0 => MotionStatus.Idle,
                            1 => MotionStatus.Running,
                            2 => MotionStatus.Stopped,
                            3 => MotionStatus.Error,
                            _ => MotionStatus.Idle
                        };
                    }
                }
            }

            return MotionStatus.Idle;
        }

        public byte[] EncodeCustom(string command, params object[] args)
        {
            string formatted = args.Length > 0 ? string.Format(CultureInfo.InvariantCulture, command, args) : command;
            return Encoding.ASCII.GetBytes($"{formatted}{TERMINATOR}");
        }
    }
}
