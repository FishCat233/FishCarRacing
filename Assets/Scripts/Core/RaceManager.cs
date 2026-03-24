using FSM.Core;
using FSM.States;
using Tools;
using UnityEngine;

public class RaceManager : Singleton<RaceManager>
{
    [Header("Race Flow Durations")]
    [SerializeField] private float preRaceDuration = 1f;
    [SerializeField] private float countdownDuration = 3f;
    [SerializeField] private float raceDuration = 15f;

    private StateMachine<RaceFlowState> _stateMachine;
    private float _stateElapsedTime;
    private float _raceElapsedTime;

    public float PreRaceDuration => preRaceDuration;
    public float CountdownDuration => countdownDuration;
    public float RaceDuration => raceDuration;
    public float StateElapsedTime => _stateElapsedTime;
    public float RaceElapsedTime => _raceElapsedTime;

    private void Start()
    {
        InitializeStateMachine();
    }

    private void Update()
    {
        if (_stateMachine == null)
        {
            return;
        }

        _stateElapsedTime += Time.deltaTime;

        if (_stateMachine.HasCurrentState && _stateMachine.CurrentStateId == RaceFlowState.Racing)
        {
            _raceElapsedTime += Time.deltaTime;
        }

        _stateMachine.Tick(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (_stateMachine == null)
        {
            return;
        }

        _stateMachine.FixedTick(Time.fixedDeltaTime);
    }

    public void ResetStateTimer()
    {
        _stateElapsedTime = 0f;
    }

    public void ResetRaceTimer()
    {
        _raceElapsedTime = 0f;
    }

    public bool ChangeFlowState(RaceFlowState targetState)
    {
        if (_stateMachine == null)
        {
            return false;
        }

        return _stateMachine.ChangeState(targetState);
    }

    private void InitializeStateMachine()
    {
        _stateMachine = new StateMachine<RaceFlowState>();

        _stateMachine.RegisterState(new PreRaceState(_stateMachine, this));
        _stateMachine.RegisterState(new CountdownState(_stateMachine, this));
        _stateMachine.RegisterState(new RacingState(_stateMachine, this));
        _stateMachine.RegisterState(new SettlementState(_stateMachine, this));

        _stateMachine.ChangeState(RaceFlowState.PreRace);
    }
}
