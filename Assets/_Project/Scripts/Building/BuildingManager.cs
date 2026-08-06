using System.Collections.Generic;
using UnityEngine;

public class BuildingManager:MonoSingleton<BuildingManager>
{
    readonly List<Building> buildings = new();
    public BuildingRegistry registry;
    [SerializeField] float tickInterval = 0.1f;
    Timer timer;

    protected override void Awake()
    {
        base.Awake();
        registry.Init();
        timer = Timer.Register(tickInterval, Tick, true);
        timer.Start();
    }
    
    public void RegisterBuilding(Building building)
    {
        buildings.Add(building);
    }
    
    public void UnregisterBuilding(Building building)
    {
        buildings.Remove(building);
    }

    void Update()
    {
        timer.Tick(Time.deltaTime);
    }
    
    void Tick()
    {
        var snapshot = new List<Building>(buildings);
        foreach (var building in snapshot)
        {
            building.OnTick(tickInterval);
        }
    }
}
