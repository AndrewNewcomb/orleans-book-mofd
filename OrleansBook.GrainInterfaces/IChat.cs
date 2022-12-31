using Orleans;

namespace OrleansBook.GrainInterfaces;

public interface IChat : IGrainObserver
{
    void ReceiveMessage(string message);
}