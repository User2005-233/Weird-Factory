using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildController : MonoSingleton<BuildController>
{
    GameControls gameControls;
    BuildMode currentMode = BuildMode.None;
    bool placeRequestFlag = false;
    BuildingDefinition definition;
    BuildingPreview preview;
    GameObject previewInstance;

    GridCoord? beltStartPoint;
    LineRenderer beltPreviewLine;
    GameObject beltPreviewGO;

    void OnEnable()
    {
        gameControls = new GameControls();
        gameControls.Enable();
        gameControls.GameActions.SelectExtractor.performed += _ => SelectBuilding("extractor");
        gameControls.GameActions.SelectFurnace.performed += _ => SelectBuilding("furnace");
        gameControls.GameActions.SelectAssembler.performed += _ => SelectBuilding("assembler");
        gameControls.GameActions.SelectStorage.performed += _ => SelectBuilding("storage");
        gameControls.GameActions.SelectConveyor.performed += _ => SelectConveyor();
        // gameControls.GameActions.SelectBeltItem.performed += OnSelectBeltItem;
        // gameControls.GameActions.SelectMachine.performed += OnSelectMachine;
        // gameControls.GameActions.SelectBuilding.performed += OnSelectBuilding;
        // gameControls.GameActions.SelectNone.performed += OnSelectNone;
        gameControls.GameActions.Place.performed += _ => placeRequestFlag = true;
        gameControls.GameActions.Cancel.performed += _ => ExitMode();
        // gameControls.GameActions.Pause.performed += OnPause;
        // gameControls.GameActions.Resume.performed += OnResume;
        // gameControls.GameActions.Quit.performed += OnQuit;
        // gameControls.GameActions.Restart.performed += OnRestart;
        // gameControls.GameActions.TogglePause.performed += OnTogglePause;
        // gameControls.GameActions.ToggleResume.performed += OnToggleResume;
        // gameControls.GameActions.ToggleCancel.performed += OnToggleCancel;
        // gameControls.GameActions.ToggleBuild.performed += OnToggleBuild;
        // gameControls.GameActions.ToggleSelectExtractor.performed += OnToggleSelectExtractor;
        // gameControls.GameActions.ToggleSelectFurnace.performed += OnToggleSelectFurnace;
        // gameControls.GameActions.ToggleSelectAssembler.performed += OnToggleSelectAssembler;
        // gameControls.GameActions.ToggleSelectConveyor.performed += OnToggleSelectConveyor;
        // gameControls.GameActions.ToggleSelectBeltItem.performed += OnToggleSelectBeltItem;
        // gameControls.GameActions.ToggleSelectMachine.performed += OnToggleSelectMachine;
        // gameControls.GameActions.ToggleSelectBuilding.performed += OnToggleSelectBuilding;
        // gameControls.GameActions.ToggleNone.performed += OnToggleNone;
    }

    void OnDisable()
    {
        gameControls?.Disable();
    }

    //choose current buildingdef and visual
    void SelectBuilding(string id){
        ExitMode();
        var selectedDef = BuildingManager.Instance.registry.GetBuilding(id);
        if(selectedDef == null)
        {
            Debug.LogError($"Building with id {id} not found");
            return;
        }
        currentMode = BuildMode.PlaceBuilding;
        definition = selectedDef;
        previewInstance = Instantiate(definition.prefab, Vector3.zero, Quaternion.identity);
        preview = previewInstance.GetComponent<BuildingPreview>();
        preview.SetValid(true);
    }

    GridCoord MouseToCoord()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        var ground = new Plane(Vector3.up, 0);          // Y=0 为网格面
        if (ground.Raycast(ray, out float dist))
        {
            Vector3 p = ray.GetPoint(dist);
            return new GridCoord(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.z));
        }
        return GridCoord.Zero;
    }

    void Update()   
    {
        if(currentMode == BuildMode.None)
        {
            return;
        }
        if(currentMode == BuildMode.PlaceBuilding)
        {
            var coord = MouseToCoord();
            preview.SetPosition(coord, definition);
            if(!GridManager.Instance.CanPlaceAt(coord, definition))
            {
                preview.SetValid(false);
                if(placeRequestFlag == true){
                    placeRequestFlag = false;
                }
            }
            else
            {
                preview.SetValid(true);
                if(placeRequestFlag == true){
                    var building = previewInstance.GetComponent<Building>();
                    GridManager.Instance.PlaceBuilding(coord, definition, building);
                    building.Initialize(coord, GridDirection.North, definition);
                    BuildingManager.Instance.RegisterBuilding(building);
                    preview.SetInPlace();
                    previewInstance = null;
                    placeRequestFlag = false;
                    currentMode = BuildMode.None;
                }
            }
        }
        if(currentMode == BuildMode.PlaceBelt)
        {
            var coord = MouseToCoord();
            if(beltStartPoint == null)
            {
                beltPreviewLine.positionCount = 0;
                if(placeRequestFlag)
                {
                    beltStartPoint = coord;
                    placeRequestFlag = false;
                }
            }
            else
            {
                UpdateBeltPreview(beltStartPoint.Value, coord);
                if(placeRequestFlag)
                {
                    TryPlaceBelt(beltStartPoint.Value, coord);
                    placeRequestFlag = false;
                    ExitMode();
                }
            }
        }
    }
    
    void ExitMode(){
        if(previewInstance != null)
        {
            Destroy(previewInstance);
        }
        previewInstance = null;
        preview = null;
        definition = null;
        currentMode = BuildMode.None;
        beltStartPoint = null;
        if(beltPreviewGO != null)
            beltPreviewGO.SetActive(false);
    }

    void SelectConveyor()
    {
        ExitMode();
        currentMode = BuildMode.PlaceBelt;
        beltStartPoint = null;
        if(beltPreviewGO == null)
        {
            beltPreviewGO = new GameObject("BeltPreviewLine");
            beltPreviewLine = beltPreviewGO.AddComponent<LineRenderer>();
            beltPreviewLine.startWidth = 0.15f;
            beltPreviewLine.endWidth = 0.15f;
            beltPreviewLine.material = new Material(Shader.Find("Sprites/Default"));
            beltPreviewLine.positionCount = 0;
        }
        beltPreviewGO.SetActive(true);
    }

    void UpdateBeltPreview(GridCoord start, GridCoord end)
    {
        if(start.z != end.z && start.x != end.x)
        {
            beltPreviewLine.positionCount = 0;
            return;
        }

        var path = BuildPath(start, end);
        bool valid = true;
        foreach(var cell in path)
        {
            if(!GridManager.Instance.CanPlaceBeltAt(cell))
            {
                valid = false;
                break;
            }
        }

        var color = valid ? Color.green : Color.red;
        beltPreviewLine.startColor = color;
        beltPreviewLine.endColor = color;
        beltPreviewLine.positionCount = path.Count;
        for(int i = 0; i < path.Count; i++)
            beltPreviewLine.SetPosition(i, new Vector3(path[i].x, 0.06f, path[i].z));
    }

    void TryPlaceBelt(GridCoord start, GridCoord end)
    {
        if(start.z != end.z && start.x != end.x)
        {
            Debug.Log("Only straight belts supported");
            return;
        }

        var path = BuildPath(start, end);

        foreach(var cell in path)
        {
            if(!GridManager.Instance.CanPlaceBeltAt(cell))
            {
                Debug.Log("Belt path blocked");
                return;
            }
        }

        GridDirection dir;
        if(start.x == end.x)
            dir = start.z < end.z ? GridDirection.North : GridDirection.South;
        else
            dir = start.x < end.x ? GridDirection.East : GridDirection.West;

        var line = new BeltLine();

        for(int i = 0; i < path.Count - 1; i++)
        {
            var seg = new BeltSegment
            {
                start = path[i],
                end = path[i + 1],
                direction = dir,
                length = 1f
            };
            line.AddSegment(seg);
            GridManager.Instance.PlaceBelt(path[i], seg);
        }

        var sourceCell = path[0] - dir.ToOffset();
        var sourceBuilding = GridManager.Instance.GetBuildingAt(sourceCell);
        if(sourceBuilding is IOutputPortProvider outProv && outProv.HasOutputPort(dir))
            line.outputProvider = outProv;

        var destCell = path[^1] + dir.ToOffset();
        var destBuilding = GridManager.Instance.GetBuildingAt(destCell);
        if(destBuilding is IInputPortProvider inProv && inProv.HasInputPort(dir.Opposite()))
            line.inputProvider = inProv;

        BeltLineManager.Instance.RegisterLine(line);
    }

    List<GridCoord> BuildPath(GridCoord start, GridCoord end)
    {
        var path = new List<GridCoord>();
        if(start.x == end.x)
        {
            int step = start.z < end.z ? 1 : -1;
            for(int z = start.z; z != end.z + step; z += step)
                path.Add(new GridCoord(start.x, z));
        }
        else
        {
            int step = start.x < end.x ? 1 : -1;
            for(int x = start.x; x != end.x + step; x += step)
                path.Add(new GridCoord(x, start.z));
        }
        return path;
    }

}