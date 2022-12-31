using OrleansBook.GrainInterfaces;

namespace OrleansBook.Client;

public class Chat : IChat
{
    public void ReceiveMessage(string message)
    {
        Console.WriteLine(message);
    }
}