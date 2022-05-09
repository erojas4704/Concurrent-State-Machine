namespace Actor
{

    public abstract class State
    {
        public int Priority;

        public delegate void NextStateCallback(Actor actor, Action action);
        public delegate void EnterStateCallback<TState>();

        public delegate void ExitStateCallback();
        public delegate void ExitStateCallback<TState>();

        public virtual void Init(Actor actor) { }
        public virtual void Update(Actor actor) { }
        public virtual void Process(Actor actor, Action action) { }
        public virtual void End(Actor actor) { }

        public EnterStateCallback<State> Enter;
        public ExitStateCallback Exit;
        public ExitStateCallback<State> ExitState;
        public NextStateCallback Next = (a, action) => { };

        public State() { }
    }
}