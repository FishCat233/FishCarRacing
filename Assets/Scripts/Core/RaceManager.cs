using FSM.Core;
using FSM.States;
using FishCarRacing.Player;
using Tools;
using UnityEngine;

public class RaceManager : Singleton<RaceManager>
{
    [Header("Race Flow Durations")]
    [SerializeField] private float preRaceDuration = 1f;
    [SerializeField] private float countdownDuration = 3f;
    [SerializeField] private float raceDuration = 15f;

    [Header("Rule System")]
    [SerializeField] private RaceRuleSystem raceRuleSystem;

    private StateMachine<RaceFlowState> _stateMachine;
    private float _stateElapsedTime;
    private float _raceElapsedTime;

    public float PreRaceDuration => preRaceDuration;
    public float CountdownDuration => countdownDuration;
    public float RaceDuration => raceDuration;
    public float StateElapsedTime => _stateElapsedTime;
    public float RaceElapsedTime => _raceElapsedTime;
    public bool HasRuleDrivenRaceFinished => raceRuleSystem != null && raceRuleSystem.AreAllRacersFinished;

    private void Start()
    {
        if (raceRuleSystem == null)
        {
            raceRuleSystem = FindFirstObjectByType<RaceRuleSystem>();
        }

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

    public void PrepareRuleSystem()
    {
        raceRuleSystem?.PrepareRace();
    }

    public void StartRuleSystem()
    {
        raceRuleSystem?.StartRace();
    }

    public void StopRuleSystem()
    {
        raceRuleSystem?.StopRace();
    }

    public bool TryProcessCheckpoint(CheckPointTrigger checkpoint, Collider other)
    {
        if (raceRuleSystem == null)
        {
            return false;
        }

        return raceRuleSystem.TryPassCheckpoint(checkpoint, other, _raceElapsedTime);
    }

    public void SetAllPlayerInputEnabled(bool enabled)
    {
        var players = FindObjectsOfType<CarController>();
        foreach (var player in players)
        {
            player.SetInputEnabled(enabled);
        }
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
