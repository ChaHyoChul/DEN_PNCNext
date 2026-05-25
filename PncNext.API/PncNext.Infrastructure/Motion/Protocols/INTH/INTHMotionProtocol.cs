using PncNext.Domain.Interfaces;
using PncNext.Domain.Models;
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

        public MotionCommandInfo EncodeMove(double x, double y, double z, double a, double b)
        {
            string cmdKey = "MOV";
            string command = $"{HEADER}|{cmdKey}|{x:F3}|{y:F3}|{z:F3}|{a:F3}|{b:F3}{FOOTER}";
            return new MotionCommandInfo {
                CommandKey = cmdKey,
                Payload = Encoding.ASCII.GetBytes(command),
                TimeoutMs = 5000
            };
        }

        public MotionCommandInfo EncodeStop()
        {
            string cmdKey = "STP";
            string command = $"{HEADER}|{cmdKey}{FOOTER}";
            return new MotionCommandInfo {
                CommandKey = cmdKey,
                Payload = Encoding.ASCII.GetBytes(command),
                TimeoutMs = 2000
            };
        }

        public MotionCommandInfo EncodeStatusRequest()
        {
            string cmdKey = "GET_STS";
            string command = $"{HEADER}|{cmdKey}{FOOTER}";
            return new MotionCommandInfo {
                CommandKey = cmdKey,
                Payload = Encoding.ASCII.GetBytes(command),
                TimeoutMs = 1000
            };
        }

        public string ExtractCommandKey(byte[] response)
        {
            if (response == null || response.Length == 0) return string.Empty;
            string resStr = Encoding.ASCII.GetString(response);
            var parts = resStr.Split('|');
            return parts.Length > 1 ? parts[1] : string.Empty;
        }

        public MotionStatus DecodeStatus(byte[] response)
        {
            if (response == null || response.Length == 0)
                return MotionStatus.Error;

            string resStr = Encoding.ASCII.GetString(response);

            if (resStr.Contains("RUNNING")) return MotionStatus.Running;
            if (resStr.Contains("IDLE")) return MotionStatus.Ready; // Idle -> Ready
            if (resStr.Contains("ALARM")) return MotionStatus.Error;
            if (resStr.Contains("PAUSE")) return MotionStatus.Pause; // Pause -> Pause

            return MotionStatus.Ready;
        }

        public MotionCommandInfo EncodeCustom(string command, params object[] args)
        {
            string cmd = args.Length > 0 ? string.Format(command, args) : command;
            string cmdKey = cmd.Split('|')[0];
            return new MotionCommandInfo {
                CommandKey = cmdKey,
                Payload = Encoding.ASCII.GetBytes($"{HEADER}|{cmd}{FOOTER}"),
                TimeoutMs = 3000
            };
        }
    }
}
