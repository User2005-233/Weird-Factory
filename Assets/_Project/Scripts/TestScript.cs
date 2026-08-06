using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    void Awake()
    {
        _ = GridManager.Instance;
    }

    void Start()
    {
        EventBus.Publish(new TestEvent(42));
    }
}
