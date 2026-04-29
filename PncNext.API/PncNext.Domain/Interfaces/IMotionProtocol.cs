namespace PncNext.Domain.Interfaces
{
    public interface IMotionProtocol
    {
        byte[] EncodeMove(double x, double y, double z, double a, double b);
        byte[] EncodeStop();
        byte[] EncodeStatusRequest();
        MotionStatus DecodeStatus(byte[] response);
    }
}
