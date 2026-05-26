using PncNext.Domain.Interfaces;
using PncNext.Domain.Models;
using System.Text;

namespace PncNext.Infrastructure.Motion.Protocols.Dummy
{
    public class DummyProtocol : IMotionProtocol
    {
        public MotionCommandInfo EncodeStatusRequest()
        {
            string cmdKey = "STATUS?";
            return new MotionCommandInfo {
                CommandKey = cmdKey,
                Payload = Encoding.ASCII.GetBytes(cmdKey),
                TimeoutMs = 1000
            };
        }

        public string ExtractCommandKey(byte[] response)
        {
            if (response == null || response.Length == 0) return string.Empty;
            string resStr = Encoding.ASCII.GetString(response);
            return resStr.Split(' ')[0];
        }

        public MotionStatus DecodeStatus(byte[] response)
        {
            var str = Encoding.ASCII.GetString(response);
            if (str.Contains("IDLE")) return MotionStatus.Ready;
            if (str.Contains("RUNNING")) return MotionStatus.Running;
            if (str.Contains("ERROR")) return MotionStatus.Error;
            return MotionStatus.Ready;
        }

        public MotionCommandInfo EncodeCustom(string command, params object[] args)
        {
            string cmd = args.Length > 0 ? string.Format(command, args) : command;
            return new MotionCommandInfo {
                CommandKey = cmd.Split(' ')[0],
                Payload = Encoding.ASCII.GetBytes(cmd),
                TimeoutMs = 3000
            };
        }
    }
}
