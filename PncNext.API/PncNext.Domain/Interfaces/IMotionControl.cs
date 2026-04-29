namespace PncNext.Domain.Interfaces
{
    public enum MotionStatus
    {
        Idle,
        Running,
        Error,
        Stopped
    }

    public interface IMotionControl
    {
        Task MoveAsync(double x, double y, double z, double a, double b);
        Task StopAsync();
        Task<MotionStatus> GetStatusAsync();
    }
}
