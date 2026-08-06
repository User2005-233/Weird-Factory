using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeltLineManager : MonoSingleton<BeltLineManager>
{
    public IReadOnlyList<BeltLine> BeltLines => beltLines;
    List<BeltLine> beltLines = new List<BeltLine>();
    float tickInterval = 0.1f;
    Timer timer;

    void Awake()
    {
        base.Awake();
        timer = Timer.Register(tickInterval, OnTick, true);
        timer.Start();
    }

    void Update()
    {
        timer.Tick(Time.deltaTime);
    }

    void OnTick()
    {
        foreach (var beltLine in beltLines)
        {
            beltLine.Tick(tickInterval);
        }
    }
    
    public void RegisterLine(BeltLine beltLine)
    {
        beltLines.Add(beltLine);
    }
    
    public void UnregisterLine(BeltLine beltLine)
    {
        beltLines.Remove(beltLine);
    }
}
