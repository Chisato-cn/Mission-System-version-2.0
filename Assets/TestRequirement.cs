using System;
using Tomoe.MissionSystem.Runtime;

[Serializable]
public class TestRequirement : MissionRequirement
{
    public int testValue;
    public bool testbool;
    
    protected override Type handlerType => typeof(TestRequirement);
    public override string Description => ".........";
    
    

    public override bool CheckMessage(object message)
    {
        throw new NotImplementedException();
    }
}
