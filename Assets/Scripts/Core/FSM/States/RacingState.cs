using FSM.Core;
using UnityEngine;

namespace FSM.States
{
    public sealed class RacingState : BaseState<RaceFlowState>
    {
        private readonly RaceManager _raceManager;

        public override RaceFlowState Id => RaceFlowState.Racing;

        public RacingState(BaseFSM<RaceFlowState> machine, RaceManager raceManager) : base(machine)
        {
            _raceManager = raceManager;
        }

        public override void Enter()
        {
            _raceManager.ResetStateTimer();
            _raceManager.ResetRaceTimer();
            _raceManager.SetAllPlayerInputEnabled(true);
            Debug.Log("[FSM] Enter Racing: unlock control and start race timer.");
        }

        public override void Update(float deltaTime)
        {
            if (_raceManager.RaceElapsedTime >= _raceManager.RaceDuration)
            {
                Machine.ChangeState(RaceFlowState.Settlement);
            }
        }
    }
}