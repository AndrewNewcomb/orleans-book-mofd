namespace OrleansBook.GrainInterfaces;

public class InstructionMessage
{
    public InstructionMessage()
    { 
        Instruction = "";
        Robot = "";       
    }

    public InstructionMessage(string instruction, string robot)
    {
        Instruction = instruction;
        Robot = robot;
    }

    public string Instruction { get; }
    public string Robot { get; }
}