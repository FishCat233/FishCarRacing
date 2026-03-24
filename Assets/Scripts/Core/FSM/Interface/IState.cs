namespace FSM
{
    public interface IState<TStateID>
    {
        TStateID Id { get; }

        void Enter();
        void Update(float deltaTime);
        void FixedUpdate(float fixedDeltaTime);
        void Exit();
        bool CanExitTo(TStateID targetStateId);

    }
}