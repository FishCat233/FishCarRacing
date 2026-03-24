namespace FSM.Core
{
    public abstract class BaseState<TStateID> : IState<TStateID>
    {
        protected readonly BaseFSM<TStateID> Machine;

        public abstract TStateID Id { get; }

        protected BaseState(BaseFSM<TStateID> machine)
        {
            Machine = machine;
        }

        public virtual void Enter() { }
        public virtual void Update(float deltaTime) { }
        public virtual void FixedUpdate(float fixedDeltaTime) { }
        public virtual void Exit() { }
        public virtual bool CanExitTo(TStateID targetStateId) => true;
    }
}