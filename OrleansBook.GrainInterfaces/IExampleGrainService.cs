using Orleans.Services;

namespace OrleansBook.GrainInterfaces;

public interface IExampleGrainService : IGrainService
{
    Task DoSomething();
}