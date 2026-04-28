using System;
using Tomoe.MissionSystem.Runtime;

[Serializable]
public class TestRequirement : MissionRequirement
{
    public int testValue;
    
    protected override Type handlerType { get; }
    public override bool CheckMessage(object message)
    {
        throw new NotImplementedException();
    }
}
