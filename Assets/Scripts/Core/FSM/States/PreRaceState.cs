using FSM.Core;
using UnityEngine;

namespace FSM.States
{
    public sealed class PreRaceState : BaseState<RaceFlowState>
    {
        private readonly RaceManager _raceManager;

        public override RaceFlowState Id => RaceFlowState.PreRace;

        public PreRaceState(BaseFSM<RaceFlowState> machine, RaceManager raceManager) : base(machine)
        {
            _raceManager = raceManager;
        }

        public override void Enter()
        {
            _raceManager.ResetStateTimer();
            _raceManager.SetAllPlayerInputEnabled(false);
            Debug.Log("[FSM] Enter PreRace: prepare racers and lock input.");
        }

        public override void Update(float deltaTime)
        {
            if (_raceManager.StateElapsedTime >= _raceManager.PreRaceDuration)
            {
                Machine.ChangeState(RaceFlowState.Countdown);
            }
        }
    }
}

    

