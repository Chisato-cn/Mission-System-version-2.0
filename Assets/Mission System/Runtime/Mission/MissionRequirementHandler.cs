namespace Tomoe.MissionSystem.Runtime
{
    public abstract class MissionRequirementHandler
    {
        protected readonly MissionRequirement requirement;
        
        protected MissionRequirementHandler(MissionRequirement requirement) => this.requirement = requirement;

        public bool SendMessage(object message, out bool hasStatusChanged)
        {
            hasStatusChanged = false;
            if (!requirement.CheckMessage(message)) return false;
            hasStatusChanged = true;
            return UseMessage(message);
        }
         
        protected abstract bool UseMessage(object message);
    }
}