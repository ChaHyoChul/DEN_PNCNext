namespace PncNext.Domain.Interfaces
{
    public enum MotionStatus
    {
        NotReady,   // Homing Àü 
        Ready,      //  
        Running,    // 
        Pause,      // 
        Error       // 
    }

    public interface IMotionControl
    {
        Task MoveAsync(double x, double y, double z, double a, double b);
        Task StopAsync();
        Task<MotionStatus> GetStatusAsync();
    }
}
