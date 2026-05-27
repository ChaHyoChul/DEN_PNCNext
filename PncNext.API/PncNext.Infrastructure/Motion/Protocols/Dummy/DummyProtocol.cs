using PncNext.Domain.Interfaces;
using PncNext.Domain.Models;
using System.Text;

namespace PncNext.Infrastructure.Motion.Protocols.Dummy
{
    public class DummyProtocol : IMotionProtocol
    {
        public MotionCommandInfo EncodeStop(int mode)
        {
            string cmdKey = "STOP";
            return new MotionCommandInfo {
                CommandKey = cmdKey,
                Payload = Encoding.ASCII.GetBytes($"{cmdKey} {mode}"),
                TimeoutMs = 1000
            };
        }

        public MotionCommandInfo EncodeHalt()
        {
            string cmdKey = "HALT";
            return new MotionCommandInfo {
                CommandKey = cmdKey,
                Payload = Encoding.ASCII.GetBytes(cmdKey),
                TimeoutMs = 1000
            };
        }

        public MotionCommandInfo EncodeErrorReset()
        {
            string cmdKey = "RESET";
            return new MotionCommandInfo {
                CommandKey = cmdKey,
                Payload = Encoding.ASCII.GetBytes(cmdKey),
                TimeoutMs = 1000
            };
        }

        public MotionCommandInfo EncodeInitController()
        {
            string cmdKey = "INIT";
            return new MotionCommandInfo {
                CommandKey = cmdKey,
                Payload = Encoding.ASCII.GetBytes(cmdKey),
                TimeoutMs = 1000
            };
        }

        public MotionCommandInfo EncodeHome()
        {
            string cmdKey = "HOME";
            return new MotionCommandInfo {
                CommandKey = cmdKey,
                Payload = Encoding.ASCII.GetBytes(cmdKey),
                TimeoutMs = 5000 // 시뮬레이션은 빠르게
            };
        }

        public MotionCommandInfo EncodeMode(string mode)
        {
            string cmdKey = "MODE";
            return new MotionCommandInfo {
                CommandKey = cmdKey,
                Payload = Encoding.ASCII.GetBytes($"{cmdKey} {mode}"),
                TimeoutMs = 1000
            };
        }

        public MotionCommandInfo EncodePause()
        {
            string cmdKey = "PAUSE";
            return new MotionCommandInfo {
                CommandKey = cmdKey,
                Payload = Encoding.ASCII.GetBytes(cmdKey),
                TimeoutMs = 1000
            };
        }

        public MotionCommandInfo EncodeContinue()
        {
            string cmdKey = "CONT";
            return new MotionCommandInfo {
                CommandKey = cmdKey,
                Payload = Encoding.ASCII.GetBytes(cmdKey),
                TimeoutMs = 1000
            };
        }

        public MotionCommandInfo EncodeMda(string gcode)
        {
            string cmdKey = "MDA";
            return new MotionCommandInfo {
                CommandKey = cmdKey,
                Payload = Encoding.ASCII.GetBytes($"{cmdKey} {gcode}"),
                TimeoutMs = 1000
            };
        }

        public MotionCommandInfo EncodeMoveIncremental(double? x, double? y, double? z, double? a, double? b)
        {
            string cmdKey = "MMI";
            return new MotionCommandInfo {
                CommandKey = cmdKey,
                Payload = Encoding.ASCII.GetBytes($"{cmdKey} X{x ?? 0} Y{y ?? 0}"),
                TimeoutMs = 2000
            };
        }

        public MotionCommandInfo EncodeMoveAbsolute(double? x, double? y, double? z, double? a, double? b)
        {
            string cmdKey = "MMA";
            return new MotionCommandInfo {
                CommandKey = cmdKey,
                Payload = Encoding.ASCII.GetBytes($"{cmdKey} X{x ?? 0} Y{y ?? 0}"),
                TimeoutMs = 2000
            };
        }

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
