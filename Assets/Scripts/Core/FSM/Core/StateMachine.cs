namespace FSM.Core
{
    
    
    public enum RaceFlowState
    {
        PreRace,
        Countdown,
        Racing,
        Settlement,
    }
    
    
    public class StateMachine<TStateID> : BaseFSM<TStateID>
    {
        
    }
}