using System.Collections;
using UnityEngine;

public class CoroutineRunner : MonoSingleton<CoroutineRunner>
{
    public static Coroutine Run(IEnumerator routine)
    {
        if (!HasInstance)
        {
            var go = new GameObject("CoroutineRunner");
            var runner = go.AddComponent<CoroutineRunner>();
            DontDestroyOnLoad(go);
        }
        return Instance.StartCoroutine(routine);
    }

    public static void Halt(Coroutine coroutine)
    {
        if (HasInstance && coroutine != null)
            Instance.StopCoroutine(coroutine);
    }

    public static void HaltAll()
    {
        if (HasInstance)
            Instance.StopAllCoroutines();
    }

    public static Coroutine Wait(float seconds, System.Action onComplete)
    {
        return Run(WaitRoutine(seconds, onComplete));
    }

    private static IEnumerator WaitRoutine(float seconds, System.Action onComplete)
    {
        yield return new WaitForSeconds(seconds);
        onComplete?.Invoke();
    }
}
