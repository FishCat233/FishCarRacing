using FSM.Core;
using UnityEngine;

namespace FSM.States
{
    public sealed class CountdownState : BaseState<RaceFlowState>
    {
        private readonly RaceManager _raceManager;

        public override RaceFlowState Id => RaceFlowState.Countdown;

        public CountdownState(BaseFSM<RaceFlowState> machine, RaceManager raceManager) : base(machine)
        {
            _raceManager = raceManager;
        }

        public override void Enter()
        {
            _raceManager.ResetStateTimer();
            _raceManager.SetAllPlayerInputEnabled(false);
            Debug.Log("[FSM] Enter Countdown: 3-2-1-Go.");
        }

        public override void Update(float deltaTime)
        {
            if (_raceManager.StateElapsedTime >= _raceManager.CountdownDuration)
            {
                Machine.ChangeState(RaceFlowState.Racing);
            }
        }
    }
}