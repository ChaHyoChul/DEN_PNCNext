namespace PncNext.Domain.Interfaces
{
    public interface ICommPort
    {
        Task OpenAsync();
        Task CloseAsync();
        Task SendAsync(byte[] data);
        Task<byte[]> ReceiveAsync();
        bool IsOpen { get; }
    }
}
