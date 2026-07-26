using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<Type, Delegate> _events = new Dictionary<Type, Delegate>();

    public static void Subscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        if (_events.TryGetValue(type, out var existing))
        {
            _events[type] = Delegate.Combine(existing, handler);
        }
        else
        {
            _events[type] = handler;
        }
    }

    public static void Unsubscribe<T>(Action<T> handler)
    {
        var type = typeof(T);
        if (_events.TryGetValue(type, out var existing))
        {
            var removed = Delegate.Remove(existing, handler);
            if (removed == null)
                _events.Remove(type);
            else
                _events[type] = removed;
        }
    }

    public static void Publish<T>(T eventArgs)
    {
        var type = typeof(T);
        if (_events.TryGetValue(type, out var del))
        {
            (del as Action<T>)?.Invoke(eventArgs);
        }
    }

    public static void Publish<T>() where T : new()
    {
        Publish(new T());
    }

    public static bool HasSubscribers<T>()
    {
        return _events.ContainsKey(typeof(T));
    }

    public static void Clear<T>()
    {
        _events.Remove(typeof(T));
    }

    public static void ClearAll()
    {
        _events.Clear();
    }
}
