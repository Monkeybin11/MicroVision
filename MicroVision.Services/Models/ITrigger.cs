namespace MicroVision.Services
{
    public interface ITrigger
    {
        void InvokeTrigger();
        void DestroyTrigger();
        event CameraControllerTrigger.ErrorEvent OnError;
    }
}