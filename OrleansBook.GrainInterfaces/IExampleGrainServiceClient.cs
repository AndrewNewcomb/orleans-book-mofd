using Orleans.Services;

namespace OrleansBook.GrainInterfaces;

public interface IExampleGrainServiceClient : IGrainServiceClient<IExampleGrainService>, IExampleGrainService
{ }