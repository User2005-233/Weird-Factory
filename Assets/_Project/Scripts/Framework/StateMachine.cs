using System;
using System.Collections.Generic;

public interface IState
{
    void OnEnter();
    void OnExit();
    void OnUpdate(float deltaTime);
}

public class StateMachine
{
    private readonly Dictionary<Type, IState> _states = new Dictionary<Type, IState>();
    private IState _currentState;
    private Type _previousStateType;
    public Type CurrentStateType => _currentState?.GetType();
    public Type PreviousStateType => _previousStateType;

    public void AddState(IState state)
    {
        _states[state.GetType()] = state;
    }

    public void ChangeState<T>() where T : IState
    {
        var type = typeof(T);
        if (!_states.TryGetValue(type, out var newState))
        {
            throw new InvalidOperationException(
                string.Format("State [{0}] is not registered in StateMachine.", type.Name));
        }

        _previousStateType = _currentState?.GetType();
        _currentState?.OnExit();
        _currentState = newState;
        _currentState.OnEnter();
    }

    public void Update(float deltaTime)
    {
        _currentState?.OnUpdate(deltaTime);
    }

    public bool IsInState<T>() where T : IState
    {
        return _currentState is T;
    }
}

public interface IState<TContext>
{
    void OnEnter(TContext context);
    void OnExit(TContext context);
    void OnUpdate(TContext context, float deltaTime);
}

public class StateMachine<TContext>
{
    private readonly Dictionary<Type, IState<TContext>> _states = new Dictionary<Type, IState<TContext>>();
    private readonly TContext _context;
    private IState<TContext> _currentState;
    private Type _previousStateType;
    public Type CurrentStateType => _currentState?.GetType();
    public Type PreviousStateType => _previousStateType;

    public StateMachine(TContext context)
    {
        _context = context;
    }

    public void AddState(IState<TContext> state)
    {
        _states[state.GetType()] = state;
    }

    public void ChangeState<T>() where T : IState<TContext>
    {
        var type = typeof(T);
        if (!_states.TryGetValue(type, out var newState))
        {
            throw new InvalidOperationException(
                string.Format("State [{0}] is not registered in StateMachine.", type.Name));
        }

        _previousStateType = _currentState?.GetType();
        _currentState?.OnExit(_context);
        _currentState = newState;
        _currentState.OnEnter(_context);
    }

    public void Update(float deltaTime)
    {
        _currentState?.OnUpdate(_context, deltaTime);
    }

    public bool IsInState<T>() where T : IState<TContext>
    {
        return _currentState is T;
    }
}
