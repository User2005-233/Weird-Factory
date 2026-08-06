using UnityEngine;

public class BuildingPreview : MonoBehaviour
{
    //This component is attached in every building prefab to show preview
    MeshRenderer meshRenderer;
    [SerializeField] private Material normal;
    [SerializeField] private Material validMaterial;
    [SerializeField] private Material invalidMaterial;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = normal;
    }
    
    public void SetValid(bool isValid)
    {
        meshRenderer.material = isValid ? validMaterial : invalidMaterial;
    }

    public void SetInPlace()
    {
        meshRenderer.material = normal;
    }

    public void SetPosition(GridCoord coord, BuildingDefinition definition)
    {
        float x = (float)definition.footprint.x / 2f;
        float z = (float)definition.footprint.y / 2f;
        transform.position = new Vector3(coord.x + x, 0f, coord.z + z);
    }
}
