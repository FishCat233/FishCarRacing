using System.Collections.Generic;

namespace FSM
{
    public class BaseFSM<TStateID>
    {
        private readonly Dictionary<TStateID, IState<TStateID>> _states = new();
        private IState<TStateID> _currentState;

        public TStateID CurrentStateId { get; private set; }
        public TStateID PreviousStateId { get; private set; }
        public bool HasCurrentState { get; private set; }
        public bool HasPreviousState { get; private set; }

        public bool RegisterState(IState<TStateID> state)
        {
            if (state == null)
            {
                return false;
            }

            _states[state.Id] = state;
            return true;
        }

        public bool RegisterState(TStateID stateId, IState<TStateID> state)
        {
            if (state == null)
            {
                return false;
            }

            _states[stateId] = state;
            return true;
        }

        public bool ChangeState(TStateID targetStateId)
        {
            if (!_states.TryGetValue(targetStateId, out var nextState))
            {
                return false;
            }

            if (HasCurrentState)
            {
                if (EqualityComparer<TStateID>.Default.Equals(CurrentStateId, targetStateId))
                {
                    return true;
                }

                if (!_currentState.CanExitTo(targetStateId))
                {
                    return false;
                }

                _currentState.Exit();
                PreviousStateId = CurrentStateId;
                HasPreviousState = true;
            }

            _currentState = nextState;
            CurrentStateId = targetStateId;
            HasCurrentState = true;
            _currentState.Enter();
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (!HasCurrentState)
            {
                return;
            }

            _currentState.Update(deltaTime);
        }

        public void FixedTick(float fixedDeltaTime)
        {
            if (!HasCurrentState)
            {
                return;
            }

            _currentState.FixedUpdate(fixedDeltaTime);
        }

        public bool TryGetState(TStateID stateId, out IState<TStateID> state)
        {
            return _states.TryGetValue(stateId, out state);
        }

    }
}