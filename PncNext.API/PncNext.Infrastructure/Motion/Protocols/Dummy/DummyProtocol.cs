using PncNext.Domain.Interfaces;
using System.Text;

namespace PncNext.Infrastructure.Motion.Protocols.Dummy
{
    public class DummyProtocol : IMotionProtocol
    {
        public byte[] EncodeMove(double x, double y, double z, double a, double b)
        {
            return Encoding.ASCII.GetBytes($"MOVE X{x} Y{y} Z{z} A{a} B{b}");
        }

        public byte[] EncodeStop()
        {
            return Encoding.ASCII.GetBytes("STOP");
        }

        public byte[] EncodeStatusRequest()
        {
            return Encoding.ASCII.GetBytes("STATUS?");
        }

        public MotionStatus DecodeStatus(byte[] response)
        {
            var str = Encoding.ASCII.GetString(response);
            if (str.Contains("IDLE")) return MotionStatus.Idle;
            if (str.Contains("RUNNING")) return MotionStatus.Running;
            if (str.Contains("ERROR")) return MotionStatus.Error;
            return MotionStatus.Stopped;
        }
    }
}
