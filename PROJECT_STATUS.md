# Project Status (read at conversation start)

## Project
- **Name**: Weird-Factory
- **Type**: 3D top-down factory automation game (like Satisfactory / DSP)
- **Engine**: Unity 2022.3.62f3c1
- **Perspective**: Fixed 50-55° overhead, pan + zoom, no rotation
- **Grid**: XZ plane, 1 unit = 1 cell, default map 20×20

## Architecture Decisions
- **Data-driven**: Logic layer runs first, rendering layer reads from it (one-way data flow)
- **Shared buffer (Satisfactory mode)**: All input ports feed into one shared `Dictionary<ItemType, int>` per building, NOT per-slot assignment
- **No ItemStack**: Deleted. Belt items are always 1 unit (`ItemType` only). Buffers use `Dictionary<ItemType, int>`. Recipes use `ItemType[]` + `int[]` paired arrays
- **No merger/splitter in MVP**: Single-line chains only
- **Namespace**: `WeirdFactory.Core` etc.
- **Tick order**: Belts tick first → BuildingManager ticks → rendering reads data
- **Conveyor belt items**: Each `BeltItem` has `ItemType itemType` + `float progress` (unit: cells), never stacks
- **Building buffers**: `Dictionary<ItemType, int>`, merged by type
- **Recipe switching**: `Building.ClearInputBuffer()` → returns items to `PlayerInventory`

## Script Inventory (30 total)
```
Core/       GridCoord, GridDirection, GridCell, GridManager
Data/       ItemType, Recipe, BuildingDefinition
Building/   IDirectionProvider, IInputPortProvider, IOutputPortProvider,
            Building(abstract), Extractor, Furnace, Assembler, Storage,
            BuildingManager, BuildingRenderer, BuildingRegistry, PlayerInventory
Conveyor/   BeltItem, BeltSegment, BeltLine, BeltRenderer
Build/      BuildMode, BuildController, BuildPreview
Camera/     CameraController
UI/         BuildPanelUI, BuildingInfoUI, GoalTrackerUI
```

## Current Progress
- [x] Folder structure created
- [x] Core/ scripts (4) — done
- [x] Data/ scripts (3) — done
- [x] Building/ interfaces (3) — done
- [x] Building.cs — done (added InputBufferView/OutputBufferView/IsProcessing/ProcessingProgress)
- [x] Storage.cs — done (override IsProcessing)
- [x] Extractor.cs — done (override IsProcessing)
- [x] Furnace.cs — done (override IsProcessing/ProcessingProgress)
- [x] Assembler.cs — done (override IsProcessing/ProcessingProgress)
- [x] BuildingManager.cs — done
- [x] BuildingRegistry.cs — done
- [x] BuildingRenderer.cs — done (reads IsProcessing → toggle+rotate WorkingIndicator)
- [x] PlayerInventory.cs — done
- [x] Conveyor/ BeltItem.cs — done
- [x] Conveyor/ BeltSegment.cs — done
- [x] Conveyor/ BeltLine.cs — done
- [ ] Conveyor/ BeltRenderer.cs — empty
- [x] Build/ (3) — done
- [x] Camera/ (1) — done
- [ ] UI/ (3) — empty
- [x] 3 ItemType .asset (IronOre, IronPlate, Gear)
- [x] 2 Recipe .asset (SmeltIron, CraftGear)
- [x] 4 BuildingDefinition .asset (Extractor, Furnace, Assembler, Storage)
- [x] BuildingRegistry.asset 创建, 所有 SO 已拖入 (4 buildings + 2 recipes + 3 items)

### 已知待修
- Furnace/Assembler: `ClearInputBuffer()` override 应删除，用基类版本即可

## User's To-Do
- Grid Ground material with grid lines
- 8 prefab models (Extractor, Furnace, Assembler, Storage, Belt_Straight, Belt_Curve, Belt_Item, BuildPreview)
  - [x] Extractor, Furnace, Assembler, Storage (4/8, Cube/Cylinder 简模)
- 4 materials (Grid_Ground, Preview_Valid, Preview_Invalid, Belt_Arrow)
- 6+ icon sprites

## Asset Wiring
- Path: Assets/_Project/Scripts/Data/{Buildings,Recipes,Items}/
- Registry: BuildingRegistry.asset (单 .asset, 含 buildingDefinitions[]/recipes[]/itemTypes[])
- 查表 API: registry.GetBuilding(id)/GetRecipe(id)/GetItem(id)
- 注意: 文件名拼写已修正 (原 BuildindRegistry → BuildingRegistry)

## User Preferences
- Writes code themselves; I only review, explain concepts, and update plan as needed
- Prefers Satisfactory/DSP design patterns
- No comments in code unless necessary
