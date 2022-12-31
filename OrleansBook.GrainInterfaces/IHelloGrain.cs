using Orleans;

namespace OrleansBook.GrainInterfaces;

public interface IHelloGrain : IGrainWithStringKey 
{
    Task Subscribe(IChat observer);
    Task UnSubscribe(IChat observer);
    
    Task DoSomethingSlow();
}