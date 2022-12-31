using Orleans;

namespace OrleansBook.GrainInterfaces;

public interface IExampleGrain : IGrainWithStringKey
{ 
    Task<DateTime> GetDateTime();
}