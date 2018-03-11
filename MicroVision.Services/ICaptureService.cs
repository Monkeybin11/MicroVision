namespace MicroVision.Services
{
    public interface ICaptureService
    {
        void Capture(int interval, int count);
        
        bool Capturing { get; }

        void Stop();
    }
}