using System.Collections.Generic;
using UnityEngine;

public class BeltRenderer : MonoBehaviour
{
    [SerializeField] float beltWidth = 0.6f;
    [SerializeField] float beltThickness = 0.05f;
    [SerializeField] float trackYOffset = 0.025f;
    [SerializeField] float itemYOffset = 0.1f;
    [SerializeField] float itemSize = 0.25f;
    [SerializeField] Material beltMaterial;
    [SerializeField] Material itemMaterial;
    [SerializeField] int poolPreAlloc = 80;

    BeltLineManager manager;
    GameObject trackRoot;
    GameObject itemRoot;

    Dictionary<BeltSegment, GameObject> tracks = new();
    Queue<GameObject> itemPool = new();
    List<GameObject> activeItems = new();
    GameObject itemTemplate;

    void Awake()
    {
        trackRoot = new GameObject("Tracks") { transform = { parent = transform } };
        itemRoot = new GameObject("Items") { transform = { parent = transform } };

        itemTemplate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        itemTemplate.name = "ItemTemplate";
        itemTemplate.transform.localScale = Vector3.one * itemSize;
        var r = itemTemplate.GetComponent<MeshRenderer>();
        if (itemMaterial != null) r.material = itemMaterial;
        Destroy(itemTemplate.GetComponent<Collider>());
        itemTemplate.SetActive(false);
        itemTemplate.transform.SetParent(itemRoot.transform);

        for (int i = 0; i < poolPreAlloc; i++)
        {
            var go = Instantiate(itemTemplate, itemRoot.transform);
            itemPool.Enqueue(go);
        }
    }

    void Start()
    {
        manager = BeltLineManager.Instance;
    }

    void Update()
    {
        if (manager == null) return;

        var lines = manager.BeltLines;
        var aliveSegs = new HashSet<BeltSegment>();

        foreach (var line in lines)
        foreach (var seg in line.segments)
        {
            aliveSegs.Add(seg);
            if (!tracks.ContainsKey(seg))
                tracks[seg] = BuildTrack(seg);
        }

        var dead = new List<BeltSegment>();
        foreach (var kv in tracks)
            if (!aliveSegs.Contains(kv.Key))
                dead.Add(kv.Key);
        foreach (var seg in dead)
        {
            Destroy(tracks[seg]);
            tracks.Remove(seg);
        }

        foreach (var go in activeItems)
        {
            go.SetActive(false);
            itemPool.Enqueue(go);
        }
        activeItems.Clear();

        foreach (var line in lines)
        foreach (var seg in line.segments)
        foreach (var item in seg.items)
        {
            var go = RentItem();
            go.transform.position = ItemWorldPos(seg, item);
            go.SetActive(true);
            activeItems.Add(go);
        }
    }

    GameObject BuildTrack(BeltSegment seg)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = $"Track_{seg.start}_{seg.end}";
        go.transform.SetParent(trackRoot.transform);

        var a = new Vector3(seg.start.x, trackYOffset, seg.start.z);
        var b = new Vector3(seg.end.x, trackYOffset, seg.end.z);
        go.transform.position = (a + b) * 0.5f;
        go.transform.localScale = new Vector3(beltWidth, beltThickness, seg.length);
        go.transform.rotation = Quaternion.LookRotation(DirToVec(seg.direction), Vector3.up);

        if (beltMaterial != null)
            go.GetComponent<MeshRenderer>().material = beltMaterial;
        Destroy(go.GetComponent<Collider>());
        return go;
    }

    static Vector3 DirToVec(GridDirection d) => d switch
    {
        GridDirection.North => Vector3.forward,
        GridDirection.South => Vector3.back,
        GridDirection.East  => Vector3.right,
        GridDirection.West  => Vector3.left,
        _                   => Vector3.forward
    };

    Vector3 ItemWorldPos(BeltSegment seg, BeltItem item)
    {
        var origin = new Vector3(seg.start.x, itemYOffset, seg.start.z);
        return origin + DirToVec(seg.direction) * item.progress;
    }

    GameObject RentItem()
    {
        if (itemPool.Count > 0)
            return itemPool.Dequeue();
        return Instantiate(itemTemplate, itemRoot.transform);
    }
}
