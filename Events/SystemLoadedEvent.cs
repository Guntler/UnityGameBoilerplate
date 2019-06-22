using System;

public class SystemLoadedEvent : GameEvent
{
    public Type SystemType;
    public SystemLoadedEvent(Type type) : base()
    {
        SystemType = type;
    }
}
