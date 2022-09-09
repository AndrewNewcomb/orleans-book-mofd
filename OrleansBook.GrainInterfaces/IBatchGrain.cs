using System;
using System.Threading.Tasks;
using Orleans;

namespace OrleansBook.GrainInterfaces;

public interface IBatchGrain : IGrainWithIntegerKey
{
    [Transaction(TransactionOption.Create)]
    Task AddInstructions((string Name, string Instruction)[] values);
}