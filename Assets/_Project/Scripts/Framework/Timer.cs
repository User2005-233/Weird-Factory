using System;
using UnityEngine;

public class Timer
{
    public float Duration { get; private set; }
    public float Elapsed { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsLooping { get; private set; }
    public float Progress => Duration > 0f ? Mathf.Clamp01(Elapsed / Duration) : 0f;
    public float Remaining => Mathf.Max(0f, Duration - Elapsed);
    public bool IsFinished => !IsLooping && Elapsed >= Duration;

    public event Action OnComplete;

    public Timer(float duration, bool looping = false)
    {
        Duration = duration;
        IsLooping = looping;
        Elapsed = 0f;
        IsRunning = false;
    }

    public void Start()
    {
        IsRunning = true;
    }

    public void Stop()
    {
        IsRunning = false;
    }

    public void Pause()
    {
        IsRunning = false;
    }

    public void Resume()
    {
        IsRunning = true;
    }

    public void Reset()
    {
        Elapsed = 0f;
        IsRunning = false;
    }

    public void Restart()
    {
        Elapsed = 0f;
        IsRunning = true;
    }

    public void SetDuration(float duration)
    {
        Duration = duration;
    }

    public void Tick(float deltaTime)
    {
        if (!IsRunning)
            return;

        Elapsed += deltaTime;

        if (Elapsed >= Duration)
        {
            OnComplete?.Invoke();

            if (IsLooping)
                Elapsed -= Duration;
            else
            {
                Elapsed = Duration;
                IsRunning = false;
            }
        }
    }

    public void Complete()
    {
        Elapsed = Duration;
        IsRunning = false;
        OnComplete?.Invoke();
    }

    public void ClearCallbacks()
    {
        OnComplete = null;
    }

    public void SetElapsed(float elapsed)
    {
        Elapsed = elapsed;
    }

    public static Timer Register(float duration, Action onComplete, bool looping = false)
    {
        var timer = new Timer(duration, looping);
        timer.OnComplete += onComplete;
        return timer;
    }
}
