using UnityEngine;

public class BuildingRenderer : MonoBehaviour
{
    [SerializeField] GameObject workingIndicator;
    Building building;

    void Awake(){
        building = GetComponent<Building>();
    }
    
    void Update()
    {
        if (building == null || workingIndicator == null) return;

        bool active = building.IsProcessing;
        if (workingIndicator.activeSelf != active)
            workingIndicator.SetActive(active);

        if (active)
            workingIndicator.transform.Rotate(0f, 90f * Time.deltaTime, 0f);
    }
    
}