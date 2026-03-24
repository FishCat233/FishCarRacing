using FSM.Core;
using UnityEngine;

namespace FSM.States
{
    public sealed class SettlementState : BaseState<RaceFlowState>
    {
        private readonly RaceManager _raceManager;

        public override RaceFlowState Id => RaceFlowState.Settlement;

        public SettlementState(BaseFSM<RaceFlowState> machine, RaceManager raceManager) : base(machine)
        {
            _raceManager = raceManager;
        }

        public override void Enter()
        {
            _raceManager.ResetStateTimer();
            Debug.Log($"[FSM] Enter Settlement: final race time = {_raceManager.RaceElapsedTime:F2}s");
        }
    }
}