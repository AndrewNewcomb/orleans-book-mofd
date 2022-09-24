using System.Threading.Tasks;
using Orleans;

namespace OrleansBook.GrainInterfaces;

public interface IRobotGrain : IGrainWithStringKey
{
    [Transaction(TransactionOption.CreateOrJoin)]
    Task AddInstruction(string instruction);

    [Transaction(TransactionOption.CreateOrJoin)]
    Task<string?> GetNextInstruction();
    
    [Transaction(TransactionOption.CreateOrJoin)]
    Task<int> GetInstructionCount();

    [Transaction(TransactionOption.NotAllowed)]
    Task<bool> DoSomethingSlow(int slowTaskTimeSeconds, GrainCancellationToken cancellationToken);
}
