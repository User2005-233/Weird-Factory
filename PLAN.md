# Weird-Factory 开发计划书

---

## 一、项目概述

**类型**：3D 俯视视角工厂自动化游戏  
**玩法**：开采资源 → 传送带运输 → 机器加工 → 组装成品 → 达成目标  
**技术**：Unity 2022.3.62 + C#  
**视角**：固定 50°~55° 俯角，支持平移和缩放，不允许旋转  
**网格**：XZ 平面，1 单位 = 1 格，默认地图 20×20

---

## 二、整体文件结构

```
Assets/_Project/
├── Data/
│   ├── Items/          ← 所有 ItemType .asset 文件
│   ├── Recipes/        ← 所有 Recipe .asset 文件
│   └── Buildings/      ← 所有 BuildingDefinition .asset 文件
│
├── Scripts/
│   ├── Core/           ← 网格系统
│   ├── Data/           ← ScriptableObject 定义
│   ├── Building/       ← 建筑系统
│   ├── Conveyor/       ← 传送带系统
│   ├── Build/          ← 建造交互
│   ├── Camera/         ← 相机控制
│   └── UI/             ← 界面
│
├── Prefabs/
│   ├── Buildings/      ← 建筑预制体
│   └── Conveyors/      ← 传送带模型预制体
│
├── Materials/
│   └── ...             ← 你自制的网格地面材质、预览材质等
│
└── Scenes/
    └── Main.unity
```

---

## 三、全部脚本清单（按文件夹，共 30 个）

---

### 📁 Scripts/Core/ —— 网格系统（4 个脚本）

| 文件 | 类型 | 功能说明 |
|------|------|----------|
| `GridCoord.cs` | struct | 网格坐标 `(int x, int z)`，含 `+` `-` `==` `!=` 运算符，含静态方法判断两点是否同排/同列 |
| `GridDirection.cs` | enum | 四个方向 `North(+Z) East(+X) South(-Z) West(-X)`，含扩展方法：`ToOffset()` 返回 GridCoord、`Opposite()`、`TurnLeft()`、`TurnRight()` |
| `GridCell.cs` | class | 单个格子数据：坐标、占用类型枚举（Empty/Building/Belt）、建筑引用、BeltSegment 引用 |
| `GridManager.cs` | MonoBehaviour 单例 | 核心网格管理器。`Dictionary<GridCoord, GridCell>` 稀疏存储；方法：`IsInBounds()` `IsOccupied()` `CanPlaceAt()` `PlaceBuilding()` `RemoveBuilding()` `GetCell()` `GetNeighbor()` |

---

### 📁 Scripts/Data/ —— 数据定义层（3 个脚本）

| 文件 | 类型 | 功能说明 |
|------|------|----------|
| `ItemType.cs` | ScriptableObject | 定义一种物品类型。字段：`itemId`(string)、`itemName`(string)、`icon`(Sprite)、`meshPrefab`(GameObject, 传送带上显示的小模型)、`maxStackSize`(int) |
| `Recipe.cs` | ScriptableObject | 定义一种加工配方。字段：`recipeId`(string)、`inputTypes`(ItemType[] 原料种类)、`inputAmounts`(int[] 对应每种原料数量，数组同下标配对)、`outputTypes`(ItemType[] 产物种类)、`outputAmounts`(int[] 对应每种产物数量)、`processTime`(float)、`requiredBuildingType`(string) |
| `BuildingDefinition.cs` | ScriptableObject | 定义一种建筑类型。字段：`buildingId`(string)、`displayName`(string)、`footprint`(Vector2Int)、`inputDirections`(GridDirection[] 相对方向，只是哪些面有物理输入口，不分原料种类）、`outputDirections`(GridDirection[] 相对方向)、`prefab`(GameObject)、`previewMaterial`(Material)、`validRecipeIds`(string[]) |

---

### 📁 Scripts/Building/ —— 建筑系统（12 个脚本）

#### 接口（3 个）

| 文件 | 类型 | 功能说明 |
|------|------|----------|
| `IDirectionProvider.cs` | interface | 返回建筑当前的 `GridDirection facing` |
| `IInputPortProvider.cs` | interface | `bool HasInputPort(GridDirection worldDir)` 查询某方向是否有入口；`bool TryInsert(ItemType itemType)` 尝试放入 1 个物品，返回是否成功 |
| `IOutputPortProvider.cs` | interface | `bool HasOutputPort(GridDirection worldDir)` 查询某方向是否有出口；`bool TryExtract(out ItemType itemType)` 尝试取出 1 个物品，返回是否成功 |

#### 基类（1 个）

| 文件 | 类型 | 功能说明 |
|------|------|----------|
| `Building.cs` | abstract MonoBehaviour | 所有建筑的基类。字段：`gridPosition`(GridCoord)、`facing`(GridDirection)、`definition`(BuildingDefinition)、`inputBuffer`/`outputBuffer`(Dictionary<ItemType,int>)。只读属性：`InputBufferView`/`OutputBufferView`(IReadOnlyDictionary)、`IsProcessing`/`ProcessingProgress`(虚属性，供渲染层和UI读取)。方法：`Initialize(pos,dir,def)`、`IsInputSide(worldDir)` 和 `IsOutputSide(worldDir)` 根据定义+朝向换算、`ClearInputBuffer()` 退换原料、`abstract OnTick(float deltaTime)` |

**端口方向换算规则（封装在 Building 基类）**：  
BuildingDefinition 存的是相对方向（"建筑的正面是入口"），Building 持有实际朝向。  
例如：定义里入口是 `North`，建筑实际朝 `East` → 入口实际在 `East`。  
子类不用关心这个换算，只调用 `IsInputSide()` / `IsOutputSide()` 即可。

#### 具体建筑（4 个）

| 文件 | 类型 | 实现接口 | 功能说明 |
|------|------|----------|----------|
| `Extractor.cs` | MonoBehaviour | `IDirectionProvider, IOutputPortProvider` | 开采器。无输入，Tick 时按间隔产出 1 个原材料到内部输出缓冲区，`TryExtract()` 取走 |
| `Furnace.cs` | MonoBehaviour | `IDirectionProvider, IInputPortProvider, IOutputPortProvider` | 熔炉。一个输入口+一个输出口，统一输入缓冲区（所有入口的东西进同一个仓库）。Tick 时检查配方原料够不够→扣原料→加工计时→输出 |
| `Assembler.cs` | MonoBehaviour | `IDirectionProvider, IInputPortProvider, IOutputPortProvider` | 组装机。一个输入口+一个输出口，所有入口的东西都进一个**共用输入缓冲区**（Satisfactory 模式），不按原料种类分槽。Tick 时检查共享缓冲区里是否凑够配方需要的所有原料→扣→加工→输出 |
| `Storage.cs` | MonoBehaviour | `IDirectionProvider, IInputPortProvider, IOutputPortProvider` | 仓库。四向可入可出，共用内部缓冲区（所有入口的东西混在一起存）。不加工，纯缓冲。有最大容量限制 |

**输入输出设计原则（Satisfactory 模式）**：

- 建筑的 `inputDirections` 只表示"物理入口在哪几面"，不限制哪个口吃哪种原料
- 所有入口送入的物品统一进入一个**共用输入缓冲区**（`Dictionary<ItemType, int>`，按类型合并数量，避免同种物品重复占多个条目）
- Tick 加工时，检查共用缓冲区里的总量是否 ≥ 配方要求的每种原料量
- 与 Factorio 的分槽模式（组装机 1 号口绑死铜线、2 号口绑死铁板）不同，这是 Satisfactory / DSP 的共用池模式

#### 建筑 Tick 调度（1 个）

| 文件 | 类型 | 功能说明 |
|------|------|----------|
| `BuildingManager.cs` | MonoBehaviour 单例 | 管理所有建筑的 Tick 循环。字段：`allBuildings`(List)、`tickInterval`(0.1f 默认)、`tickTimer`(float)。在 `Update()` 中累计时间，到阈值时遍历呼唤每个 Building 的 `OnTick()` |

#### 建筑视觉（1 个）

| 文件 | 类型 | 功能说明 |
|------|------|----------|
| `BuildingRenderer.cs` | MonoBehaviour | 挂在建筑预制体上。读取 `Building.IsProcessing` 控制 `WorkingIndicator` 显隐+旋转。进度条和物品流动分别由 BuildingInfoUI / BeltRenderer 负责

#### 玩家背包（1 个）

| 文件 | 类型 | 功能说明 |
|------|------|----------|
| `PlayerInventory.cs` | MonoBehaviour 单例 | 玩家手持物品背包。内部用 `Dictionary<ItemType, int>` 按类型合并数量。提供 `Add(ItemType, int)`、`Remove(ItemType, int)`、`GetCount(ItemType)`。用途：拆除返还、切换配方退还原料、未来手动拾取 |

#### 建筑注册（1 个）

| 文件 | 类型 | 功能说明 |
|------|------|----------|
| `BuildingRegistry.cs` | ScriptableObject | `BuildingRegistry.asset` 单例资产，Inspector 拖入所有 SO 引用。提供 `Init()`(BuildingManager 调一次)、`GetBuilding(id)`、`GetRecipe(id)`、`GetItem(id)` 字典查找方法 |

#### 配方切换流程

- `Building` 基类提供虚方法 `Dictionary<ItemType, int> ClearInputBuffer()`——清空并返还输入缓冲区
- UI 中点击切换配方 → 调用 `ClearInputBuffer()` → 物品转入 `PlayerInventory` → 设置新 recipeId → 下次 Tick 按新配方加工

---

### 📁 Scripts/Conveyor/ —— 传送带系统（4 个脚本）

| 文件 | 类型 | 功能说明 |
|------|------|----------|
| `BeltItem.cs` | struct | 传送带上的物品数据。字段：`itemType`(ItemType)、`progress`(float，单位是格子数，非 0~1)。每个 BeltItem 始终只代表 1 个物品，不堆叠
| `BeltSegment.cs` | class（非 MonoBehaviour） | 一段**直线**传送带（两点之间无拐弯）。字段：`start`(GridCoord)、`end`(GridCoord)、`direction`(GridDirection)、`length`(int)、`speed`(float)、`items`(List<BeltItem>)、`upstreamProvider`(IOutputPortProvider)、`downstreamReceiver`(IInputPortProvider)。方法：`TryPushItem(item)` 尝试从头部推入、`Tick(deltaTime)` 推进所有物品（反向遍历） |
| `BeltLine.cs` | class（非 MonoBehaviour） | 由多个 BeltSegment 首尾相连组成的完整传送带线路。字段：`segments`(List<BeltSegment>)、`provider`(IOutputPortProvider)、`receiver`(IInputPortProvider)。方法：`AddSegment()`、`Tick(deltaTime)` 从尾到头依次推进每个段、`ConnectToPorts()` 自动寻找首尾连接的建筑端口 |
| `BeltRenderer.cs` | MonoBehaviour | 挂在场景中某个空物体上。持有 `BeltLine` 引用，负责渲染所有传送带模型（直段/弯角）和物品模型。物品用 `Graphics.DrawMeshInstanced` 或对象池渲染 |

**物品推进核心逻辑（BeltLine.Tick）：**

```
从尾到头反向遍历 segments：
  for each segment（反序）:
    从尾到头遍历 segment.items：
      如果 progress ≥ segment.length：
        剩余距离 = progress - segment.length
        将物品推入下一个 segment（或交给 receiver）
      否则：
        progress += speed * deltaTime
      如果被堵塞（下一段满 / receiver 满）：
        停止推进，形成背压
```

**端口自动连接逻辑（BeltLine.ConnectToPorts）：**

```
// 只在放置确认时调用一次

起点：
  遍历起点周围 4 格（GridDirection.North/East/South/West）
  每格 → GridManager.GetCell() → 是 Building？
    → 拿到 building 引用，调 building.HasOutputPort(从建筑指向起点的方向)
      → true → beltLine.provider = building（传送带从它取货）

终点：
  遍历终点周围 4 格
  每格 → GridManager.GetCell() → 是 Building？
    → 拿到 building 引用，调 building.HasInputPort(从终点指向建筑的方向)
      → true → beltLine.receiver = building（传送带向它交货）
```

**运行时取货/交货（BeltLine.Tick 每帧做）：**

```
取货：if provider != null && TryExtract(out item) → 塞入第一个 segment 头部
交货：物品推到最后一个 segment 终点 → TryInsert(item) → 成功则丢弃，失败则卡住
```

**合并器/分流器**：MVP 不包含。验收链条是单线的不需要。后续作为独立 1×1 建筑追加，不影响现有传送带系统。

---

### 📁 Scripts/Build/ —— 建造交互（3 个脚本）

| 文件 | 类型 | 功能说明 |
|------|------|----------|
| `BuildMode.cs` | enum | 建造模式枚举：`None`（默认） / `PlaceBuilding`（放置建筑） / `PlaceBelt`（放置传送带） |
| `BuildController.cs` | MonoBehaviour 单例 | 核心交互控制器。**状态机**管理建造流程、**热键处理**（1/2/3/4 切换建筑类型）、**鼠标→网格** Raycast 转换、**右键取消/退出**。字段：`currentMode`、`selectedDefinition`、`beltWaypoints`。详见下方"建造交互流程"章节 |
| `BuildPreview.cs` | MonoBehaviour | 挂在预览模型实例上。接收 `BuildController` 的指令，更新自身位置（吸附到网格中心）和材质颜色（绿/红） |

---

### 📁 Scripts/Camera/ —— 相机控制（1 个脚本）

| 文件 | 类型 | 功能说明 |
|------|------|----------|
| `CameraController.cs` | MonoBehaviour | 挂主摄像机上。**滚轮缩放**（调整 y 高度，clamp 到 min/max）、**WASD 平移**（XZ 平面移动）、**右键拖拽平移**、**边缘推动**（鼠标贴近屏幕边缘时自动平移）。Clamp 相机范围不超出地图 |

---

### 📁 Scripts/UI/ —— 用户界面（3 个脚本）

| 文件 | 类型 | 功能说明 |
|------|------|----------|
| `BuildPanelUI.cs` | MonoBehaviour | 底部建筑选择栏。显示建筑图标+热键文字，响应 BuildController 的建造模式切换。监听热键 1-4 和鼠标点击 |
| `BuildingInfoUI.cs` | MonoBehaviour | 建筑信息弹出面板。点击已放置的建筑时显示：名称、入口/出口方向标记、输入/输出缓冲区物品列表。仓库类显示所有库存 |
| `GoalTrackerUI.cs` | MonoBehaviour | 目标追踪。显示当前目标（如"齿轮：7/10"），到达目标时弹出胜利提示 |

---

## 四、ScriptableObject 数据配置文件清单

你需要**右键 → Create** 手动创建的 `.asset` 文件：

### Items/ （3 个）

| 文件名 | 对应类 | 数据内容 |
|--------|--------|----------|
| `IronOre.asset` | ItemType | id=`iron_ore`，名称="铁矿石" |
| `IronPlate.asset` | ItemType | id=`iron_plate`，名称="铁板" |
| `Gear.asset` | ItemType | id=`gear`，名称="齿轮" |

### Recipes/ （2 个）

| 文件名 | 对应类 | 数据内容 |
|--------|--------|----------|
| `SmeltIron.asset` | Recipe | 输入=铁矿石×1，输出=铁板×1，耗时=1.5s，建筑=furnace |
| `CraftGear.asset` | Recipe | 输入=铁板×2，输出=齿轮×1，耗时=2.0s，建筑=assembler |

### Buildings/ （4 个）

| 文件名 | 对应类 | 数据内容 |
|--------|--------|----------|
| `Extractor.asset` | BuildingDefinition | 占地=1×1，出口=South，无入口，预制体=Extractor.prefab |
| `Furnace.asset` | BuildingDefinition | 占地=1×1，入口=North，出口=South，配方=`smelt_iron` |
| `Assembler.asset` | BuildingDefinition | 占地=2×2，入口=North，出口=South，配方=`craft_gear` |
| `Storage.asset` | BuildingDefinition | 占地=2×2，入口=`[North, East, South, West]`，出口=`[North, East, South, West]`（四向通用） |

---

## 五、你需要手动制作的东西

### 3D 模型（Prefab，共 8 个）

> 首版可以用 Unity 内置 Cube/Cylinder 拼凑，下面给出每个模型的形状描述

| 预制体 | 形状描述 | 挂载的脚本 |
|--------|----------|------------|
| `Extractor.prefab` | 一个小底座 + 一个钻头（朝下的锥体） | `Extractor.cs`, `BuildingPreview.cs`, `BuildingRenderer.cs` |
| `Furnace.prefab` | 一个方形箱子，侧面发光孔 | `Furnace.cs`, `BuildingPreview.cs`, `BuildingRenderer.cs` |
| `Assembler.prefab` | 2×2 大平台，上面有机械臂 | `Assembler.cs`, `BuildingPreview.cs`, `BuildingRenderer.cs` |
| `Storage.prefab` | 2×2 的大箱子/仓库 | `Storage.cs`, `BuildingPreview.cs`, `BuildingRenderer.cs` |
| `Belt_Straight.prefab` | 1×0.5 的长条平台（类似跑步机） | 无脚本，纯视觉 |
| `Belt_Curve.prefab` | 1×1 的 L 形转弯平台 | 无脚本，纯视觉 |
| `Belt_Item.prefab` | 一个很小的方块/球，代表传送带上的物品 | 无脚本（或用对象池管理） |
| `BuildPreview.prefab` | 任意建筑模型替换为半透明材质 | `BuildingPreview.cs`（实际实现：BuildingPreview 组件直接挂在建筑 prefab 上，无需单独预览 prefab） |

### 材质（至少 4 个）

| 材质 | 用途 |
|------|------|
| `Grid_Ground.mat` | 铺在地面上的网格纹理，方便看到格子边界 |
| `Preview_Valid.mat` | 绿色半透明，建造预览时表示可放置 |
| `Preview_Invalid.mat` | 红色半透明，建造预览时表示不可放置 |
| `Belt_Arrow.mat` | 可选，传送带上的方向箭头贴图 |

### 图标/Sprites（至少 6 个）

| 图标 | 用途（Panel UI 显示） |
|------|----------------------|
| `Icon_Extractor.png` | 底部栏开采器按钮 |
| `Icon_Furnace.png` | 底部栏熔炉按钮 |
| `Icon_Assembler.png` | 底部栏组装机按钮 |
| `Icon_Storage.png` | 底部栏仓库按钮 |
| `Icon_Belt.png` | 底部栏传送带按钮 |
| `Icon_Gear.png` | 目标进度条旁边的小图标 |
| （可选）`IronOre.png` `IronPlate.png` `Gear.png` | 物品图标 |

### 场景设置

1. **网格地面**：自己做一个带网格线纹理的 Plane，铺在 Y=0 处
2. **主相机**：角度设置为从斜上方俯视（Rotation 约 X=50°, Y=0°, Z=0°），挂 `CameraController.cs`
3. **方向光**：一个默认 Directional Light 即可

---

## 六、分阶段实现计划

### 阶段 0 —— 极简原型（目标：开采器→直传送带→仓库 跑通） ✅ 完成

> 验收标准：Console 日志输出物品沿传送带移动，仓库计数递增

| 步骤 | 要做的 | 状态 |
|------|--------|------|
| 0.1 | 创建目录结构 + `GridCoord.cs` + `GridDirection.cs` | ✅ |
| 0.2 | `GridCell.cs` + `GridManager.cs` | ✅ |
| 0.3 | `ItemType.cs` ScriptableObject | ✅ |
| 0.4 | `BeltItem.cs` + `BeltSegment.cs` + `BeltLine.cs` — 纯数据推物品逻辑，不渲染 | ✅ |
| 0.5 | `Building.cs` 基类 + `Extractor.cs` + `Storage.cs` | ✅ |
| 0.6 | `BuildingManager.cs` — Tick 调度 | ✅ |
| 0.7 | `BuildController.cs` — 最简单的输入：按 1 放开采器，按 2 放仓库 | ✅ (建筑放置完成，传送带放置待做) |

**此阶段不涉及**：传送带渲染、UI、接口、多段折线、拐弯

---

### 阶段 1 —— 传送带可视化 + 拐弯 + 多点放置

| 步骤 | 要做的 |
|------|--------|
| 1.1 | `BeltRenderer.cs` — 渲染直段/弯角模型和物品 |
| 1.2 | `BuildController.cs` 升级 — 多点折线传送带放置（点起点→点拐角→点终点→右键确认） |
| 1.3 | `BeltLine.cs`升级 — 多段连接、拐角段自动识别 |
| 1.4 | `BuildPreview.cs` — 传送带拖拽时实时预览路径 |

---

### 阶段 2 —— 建筑端口 + 接口 + 配方 ✅ 完成

| 步骤 | 要做的 | 状态 |
|------|--------|------|
| 2.1 | `IInputPortProvider.cs` + `IOutputPortProvider.cs` + `IDirectionProvider.cs` | ✅ |
| 2.2 | 改造 `Extractor`/`Storage` 实现接口 | ✅ |
| 2.3 | `Recipe.cs` ScriptableObject + 创建 2 个配方 asset | ✅ |
| 2.4 | `Furnace.cs` + `Assembler.cs` — 配方驱动 Tick | ✅ |
| 2.5 | `BuildingDefinition.cs` ScriptableObject + 创建 4 个建筑定义 asset | ✅ |
| 2.6 | `BuildingRegistry.cs` — 建筑注册查找 | ✅ |

---

### 阶段 3 —— UI + 胜利条件

| 步骤 | 要做的 |
|------|--------|
| 3.1 | `BuildPanelUI.cs` — 底部建筑选择栏 |
| 3.2 | `BuildingInfoUI.cs` — 点击建筑弹出信息面板 |
| 3.3 | `GoalTrackerUI.cs` — 目标进度 + 胜利弹窗 |

---

### 阶段 4 —— 打磨

| 步骤 | 要做的 | 状态 |
|------|--------|------|
| 4.1 | 传送带拆除（右键点传送带格子） | |
| 4.2 | 冲突检测完善（不能覆盖已有建筑） | ✅ (CanPlaceAt 支持 footprint) |
| 4.3 | `CameraController.cs` — 缩放、平移、边缘推动 | ✅ |
| 4.4 | `BuildingRenderer.cs` — 建筑动画状态 | ✅ (读取 IsProcessing → WorkingIndicator 显隐+旋转) |
| 4.5 | 测试关卡：手动布置一个完整链（矿脉+熔炉+组装机+仓库），跑通成就 | |

---

## 七、建造交互流程详解

### 建筑放置模式

```
1. 按热键 1/2/3/4 → BuildController.currentMode = PlaceBuilding
2. 实例化 BuildPreview 对象，半透明
3. 每帧 Update：
    a. 从鼠标发射 Raycast 打到 Y=0 的 Plane
    b. 交点取整 → GridCoord
    c. BuildingPreview 吸附到该坐标中心（根据 footprint 偏移）
    d. 检查 GridManager.CanPlaceAt(coord, definition) → 绿/红色
 4. 左键点击 → GridManager.PlaceBuilding(coord, definition, building) → building.Initialize()
 5. 右键 → 退出模式
```

### 传送带放置模式（多点折线）

```
1. 按传送带热键 → currentMode = PlaceBelt, waypoints.Clear()
2. 鼠标移动 → 高亮当前格
3. 左键点击格 A → waypoints.Add(A)，A 成为锚点
4. 移动鼠标 → 实时显示从 A 到当前格的路径预览
   - 只有与 A 同排或同列才显示路径
   - 路径上的格子逐个检查合法性
5. 左键点击格 B → waypoints.Add(B)，B 成为新锚点
   - B 必须与 A 正交对齐
6. 如果继续点 C，形成折线：A→B→C
7. 右键 → 确认！
   - 创建 BeltLine，生成 BeltSegment 连接相邻锚点对
   - 自动寻找首尾建筑端口并连接
   - GridManager 标记沿途格子
8. ESC → 取消所有 waypoints
```

---

## 八、关键数据结构关系图

```
┌─────────────┐     Dictionary<GridCoord, GridCell>
│ GridManager  │─────────────────────────────────────┐
└─────────────┘                                     │
                                                    ▼
                   ┌──────────────────────────────────┐
                   │ GridCell                         │
                   │  - coord: GridCoord              │
                   │  - occupation: Empty/Building/Belt│
                   │  - buildingRef ──────────────┐   │
                   │  - beltSegmentRef ───┐       │   │
                   └──────────────────────┼───────┼───┘
                                          │       │
                          ┌───────────────┘       └──────────────┐
                          ▼                                       ▼
               ┌──────────────────┐                  ┌──────────────────┐
               │ BeltSegment      │                  │ Building (abstract)
               │  - start/end      │                  │  - gridPosition   │
               │  - items: List<>  │                  │  - facing         │
               │  - provider ◄─────┼──── IOutput      │  - definition     │
               │  - receiver ◄─────┼──── IInput       └────────┬─────────┘
               └──────────────────┘                           │
                                      ┌───────────────────────┼───────────────────────┐
                                      ▼                       ▼                       ▼
                               Extractor              Furnace/Assembler           Storage
                              (output only)          (input + output)        (input + output)
```

---

## 九、检查清单（完成度追踪）

### 需要手动制作的内容

- [ ] 网格地面材质（Grid_Ground.mat）
- [ ] 开采器 3D 模型（Extractor.prefab）
- [ ] 熔炉 3D 模型（Furnace.prefab）
- [ ] 组装机 3D 模型（Assembler.prefab）
- [ ] 仓库 3D 模型（Storage.prefab）
- [ ] 直传送带模型（Belt_Straight.prefab）
- [ ] 弯传送带模型（Belt_Curve.prefab）
- [ ] 传送带物品模型（Belt_Item.prefab）
- [ ] 预览材质 ×2（Preview_Valid.mat, Preview_Invalid.mat）
- [ ] 图标 Sprites ×6
- [ ] 3 个 ItemType .asset 文件
- [ ] 2 个 Recipe .asset 文件
- [ ] 4 个 BuildingDefinition .asset 文件

### 脚本检查清单

- [x] Core/ —— 4 个
- [x] Data/ —— 3 个
- [x] Building/ —— 12/12 个 ✅
- [x] Conveyor/ —— 3/4 个（剩 BeltRenderer）
- [x] Build/ —— 3 个
- [x] Camera/ —— 1 个
- [ ] UI/ —— 3 个
- [x] **已完成：28/30 个文件**

