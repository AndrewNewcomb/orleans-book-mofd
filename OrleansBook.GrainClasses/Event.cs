using System;

namespace OrleansBook.GrainClasses;

public interface IEvent
{    
}

public class EnqueueEvent : IEvent
{
    public string? Value {get; set;}
    public EnqueueEvent() {}
    public EnqueueEvent(string? value) => this.Value = value;
}

public class DequeueEvent: IEvent
{
    public string? Value {get; set;}
    public DequeueEvent() {}
}