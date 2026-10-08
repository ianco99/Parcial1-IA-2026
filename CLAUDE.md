# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Unity **6000.3.11f1** (URP) coursework project for *Inteligencia Artificial 2026*. The current work is the
**Primer Parcial**: a 2D ant-colony simulation (queen / workers / warriors, food, anthills) in which each agent
type is driven by a different hand-written AI pattern — FSM for the queen, Behaviour Tree for workers, HSM for
warriors — over an A* grid, plus Voronoi and flocking. All patterns must be implemented from scratch; no
third-party libraries.

`CHECKLIST-Parcial.txt` (git root) is the authoritative scope: 60 numbered items in difficulty order, each
tagged `[ENTREGA]` / `[0-4]` / `[4-7]` / `[7-10]`, with `>` lines recording the evidence for what is already
done and where to touch for what is not. Deadline 2026-10-01 18:00 ART. `Primer parcial IA 2026.pdf` is the
original brief. Read the checklist before planning work — it is kept current by hand and encodes decisions the
code does not.

The Unity project root is `InteligenciaArtificial/`; the git root is one level above it and also holds the
standalone `KuznickiAgents/` library.

## Build / run

Two things build separately, and mixing them up is the most common source of confusion.

**1. The AI library (`KuznickiAgents/`) → a DLL committed into `Assets/`.** There is no post-build step; the
copy is manual, and a stale DLL shows up as Unity compile errors against code that looks correct on disk:

```powershell
dotnet build "C:\Users\Ian\Image\2026\IA-2026\KuznickiAgents\KuznickiAgents.sln"
Copy-Item "C:\Users\Ian\Image\2026\IA-2026\KuznickiAgents\KuznickiAgents\bin\Debug\netstandard2.1\KuznickiAgents.dll" `
          "C:\Users\Ian\Image\2026\IA-2026\InteligenciaArtificial\Assets\KuznickiAgents.dll" -Force
```

Keep the existing `Assets/KuznickiAgents.dll.meta` — overwrite only the DLL, never delete the pair.

**2. The Unity project.** Work in the editor (open `InteligenciaArtificial/`). Headless compile check — the
closest thing to a lint step:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -quit `
  -projectPath "C:\Users\Ian\Image\2026\IA-2026\InteligenciaArtificial" `
  -logFile "$env:TEMP\unity-compile.log"
Select-String -Path "$env:TEMP\unity-compile.log" -Pattern "error CS"
```

Batchmode fails while the editor is open on the project (lock file) — close it first, or read errors from
`InteligenciaArtificial/Logs/`. There are no automated tests; the Test Framework package is installed, unused.

**Scenes.** Play starts at `Assets/Parcial/GameConfig.unity`: `MapConfigurator` reads map width/height, tile
size and anthill count from TMP input fields, writes them as **five** `uint`s to
`Application.persistentDataPath/Map/map_config.bin`, then loads `Game`. `Assets/Parcial/Game.unity` is the
simulation. `Scenes/SampleScene.unity`, `PathfinderScene.unity` and `Flocking/Scene/Flocking.unity` hold older
coursework exercises. `MapConfigReader.Init` throws `DataEntryException` if `map_config.bin` is missing or
shorter than 20 bytes (a file from the old four-field format — re-run `GameConfig`), and
`BrokenGameRuleException` for fewer than 2 anthills. Adding a config field means editing `MapConfig`,
`MapConfigIO.Save` (both in `MapConfigurator.cs`) and `MapConfigReader` together, in the same order.

## Architecture

### Compilation units — the layering the exam is graded on

Four assemblies, ordered from engine-free to engine-bound. The separation is a grading requirement (checklist
items 06-10), so **never** introduce a `UnityEngine` reference into the first three:

| Unit | Where | Engine refs |
|---|---|---|
| `KuznickiAgents.dll` | `KuznickiAgents/` (netstandard2.1, C# latest, nullable on), committed prebuilt to `Assets/` | none — built outside Unity |
| `ToolboxAssembly` | `Assets/ToolBox/` | `noEngineReferences: true` |
| `ArchitectureAssenbly` *(sic)* | `Assets/Parcial/Architecture/` | `noEngineReferences: true`, references ToolboxAssembly |
| `Assembly-CSharp` | everything else in `Assets/` — the whole View layer plus legacy scripts | Unity |

`Assets/ToolBox/Dependencies/` holds NPOI, MathNet, ImageSharp and friends as plain DLLs; they are the
Toolbox's own dependencies (xlsx reading) and pre-date this exam. The "no third-party libraries" rule applies to
the AI patterns themselves — do not reach for a package to implement Voronoi, flocking or pathfinding.

### `KuznickiAgents` — the engine-free AI library

Namespaces `KuznickiAgents.StatePattern`, `.StatePattern.BehaviourTree`, `.Pathfinding`. Consumed from Unity by
the legacy agents in `Assets/` and, going forward, by the ant agents.

**`BehaviourActions` is the execution contract shared by FSM and HSM** and is a prerequisite for touching
either. A state never executes work directly: `GetOnEnterBehaviours` / `GetTickBehaviours` /
`GetOnExitBehaviours` **build and return** a `BehaviourActions` struct of deferred `Action`s bucketed by an
integer execution order. `ExecuteBehaviour` (duplicated in `FSM.cs` and `HSM.cs`) walks orders `0, 1, 2, …`; for
each order it runs the multi-threadable bucket via `Parallel.ForEach` *concurrently with* the main-thread
bucket, joins, then advances. Anything touching Unity API (`Transform`, `Time`, …) must go in the main-thread
bucket; only pure computation belongs in the multi-threadable one. `SetTransitionBehaviour` runs last, after
every bucket, and is where states evaluate conditions and raise flags via `OnTrigger?.Invoke(flagName)`.

Flags are raised as **strings**; states stringify their owner's enum (e.g.
`Entities.QueenAnt.Flags.OnEggToSpawn.ToString()`), which couples state classes back to their agent. State
parameters are an untyped `params object[]` produced by the `…ParametersPointer` lambdas registered in
`AddState`, unpacked **positionally** inside the state (`parameters[0]` etc.). `Fsm.AddState` also takes
trailing `constructionParameters`, passed through `Activator.CreateInstance`, so a state that needs fixed
per-instance data (the queen's `StandByState` gets its `teamID`) declares a `(params object[])` constructor.
Adding or reordering a parameter means editing both the agent's lambda and the state's unpack block; nothing
checks this at compile time. Per-tick values that change every frame (delta time) are passed as a
`Func<…>` pointer rather than a value, because the parameter lambda is evaluated when behaviours are built.

- **`Fsm`** — flat. States are `int`, flags are strings resolved through a `Dictionary<string,int>` `flagAlias`
  given to the constructor, transitions are a dense `(destination, onTransition)[,]` table sized
  `statesAmount × flagAlias.Count`.
- **`Hsm`** — hierarchical, keyed by an opaque `NodeID` struct issued by an internal `NodeIdFactory`, so IDs
  from different `Hsm` instances never compare equal. `NodeID.ROOT_ID` is `default` and doubles as "null
  parent". Behaviours to preserve: `OnFlag` → `TryGetTransition` walks **up the parent chain**, so a transition
  registered on a parent is inherited by all descendants; `ChangeState` finds the least common ancestor via
  `FindCommonParent`, calls `ExitUntil` up to it, invokes the transition action, then `EnterFromParent` down, so
  enter/exit fire only for nodes actually crossed; destinations **must be leaves** and self-transitions throw;
  `ForceState` descends to the default leaf, which is simply `children[0]` all the way down; flags must be
  registered with `AddFlagAlias` before `RegisterTransition`.
- **Behaviour Tree** — a separate lineage from the FSM/HSM states. Nodes derive from `BTNode` and implement
  `NodeState Run(Blackboard)` returning `Success` / `Failure` / `Running`; trees are composed in the agent's
  `Awake` and `BehaviourTree.Tick()` walks them synchronously. `ParallelNode` fans children out over
  `Blackboard.ParallelOptions`. `Blackboard` is a `string → object` dictionary plus a parallel
  `IBlackboardReference` dictionary, created by `BehaviourTree` and threaded through every `Run`, though most
  existing nodes capture dependencies in their constructor and ignore it.
- **Pathfinding** — `Pathfinder<TNodeType>` where `TNodeType : IPathfindingNode`. Usage is always the same:
  construct a pathfinder with the node collection, call `FindPath(start, destination, nodes)`, get the path
  without the start node and including the destination, or `null`. `FindPath` is `virtual`: the base body is a
  best-first loop (open/closed lists, `AcumulativeCost + Heuristic` selection, local `GeneratePath` backtracking)
  that `AStarPathfinder` and `DijstraPathfinder` inherit — Dijkstra is the same loop with `Distance` returning 0 —
  while `BreadthFirstPathfinder` (queue) and `DepthFirstPathfinder` (stack) override it. Each class derives
  directly from `Pathfinder` and repeats the grid hooks (`GetNeighbors`, `Distance`, `NodesEquals`,
  `MoveToNeighborCost`, `IsBlocked`) on purpose, so each file is one complete algorithm. `FindPath` keeps no
  instance state because it is called from worker threads. `ThetaStarPathfinder` runs the base A* loop and then
  straightens the result: from each path node it jumps to the farthest one with a Bresenham line of sight, but only
  through nodes passing the virtual `CanOptimizeThrough` (the Architecture subclass restricts it to the team's own
  anthill tiles), so its paths contain diagonal steps. The `PathfindingAlgorithm` enum names the five; each
  `LivingEntity` carries one in its `PathfindingAlgorithm` field and the Architecture `Pathfinding` service holds
  one pathfinder per team × algorithm (A*, Theta* and Dijkstra subclassed for per-team tile cost). `Pathfinder.cs` and
  the algorithm classes carry explanatory comments in Spanish.

### `ianco99.ToolBox` (`Assets/ToolBox/`) — the service substrate

Everything below hangs off these; they are engine-free and reused by both Architecture and View.

- **`ServiceProvider.Instance`** — a lazy singleton `Type → IService` registry. Systems do not take
  constructor dependencies; they declare `private Foo Foo => ServiceProvider.Instance.GetService<Foo>();` and
  resolve lazily at each call. `IService.IsPersistance` decides whether `ClearAllNonPersistanceServices` keeps
  the instance across scenes. `AddService` silently no-ops if the type is already registered.
- **`EventBus`** — the spine of the Architecture↔View split. Events are `struct : IEvent` (which is
  `IResettable`), pooled through `ConcurrentPool`, and **assigned positionally from `params object[]`**:
  `Raise<TEvent>(a, b, c)` pulls an instance from the pool, calls `Assign(parameters)`, invokes every
  subscriber with `in`, then releases it. Handlers take `(in TEvent)` and must not retain the struct.
  **There are two kinds of subscription, and this is what lets states raise events from worker threads.**
  `Subscribe` handlers run synchronously on whatever thread called `Raise` — including a `Parallel.ForEach`
  bucket — so they must be thread-safe and engine-free. `SubscribeOnlyMainThread` handlers are always
  enqueued (even when raised on the main thread) and drained in raise order by `EventBus.Tick`, which
  `Gameplay.Tick` calls last; view code touching Unity API should subscribe this way. Unsubscribe with the
  matching `UnSubscribe` / `UnsubscribeOnlyMainThread`.
- **`BlueprintRegistry` / `BlueprintBinder` / `BlueprintParameterAttribute`** — game data lives in
  `Assets/StreamingAssets/Blueprints/Blueprints.xlsx`, one sheet per table. `BlueprintRegistry` loads every
  sheet at construction via NPOI; `BlueprintBinder.Apply(ref object instance, table, blueprintId)` walks the
  instance's fields up the inheritance chain and fills each `[BlueprintParameter("Column Header")]` field by
  `StringCast.Convert` from the cell. Binding targets are typically small structs (`TileData`,
  `PrefabsRegistryView.PrefabPath`, `MapView.TileViewData`) — note `Apply` takes `ref object`, so they are
  boxed, bound, then cast back. A missing or mistyped column surfaces as `DataEntryException` naming the
  table and row. Table name constants live in `Parcial/Architecture/TableNames.cs` and
  `Parcial/View/TableNamesView.cs`; adding a table means adding both the sheet and the constant.
- **`TaskScheduler`** — `Schedule(Action, delay)`, ticked with logic delta time; use it for cooldowns
  (egg-laying, attack recovery) rather than coroutines, which would bind logic to the engine.
- **`IInitable` (`Init` + `LateInit`) / `ITickable` (`Tick(float)`)** — the two-phase startup used everywhere:
  `Init` registers services and builds objects, `LateInit` runs once every service exists.

### `Parcial.Architecture` — the simulation, with no engine

Driven from Unity but unaware of it. `Gameplay` is the composition root: its constructor registers `EventBus`,
the three blueprint services, `TaskScheduler`, `MapConfigReader`, `Scene` and `Time`; `Init` reads
`map_config.bin` (`MapConfigReader.Init`) then `Scene.Init` registers `EntityRegistry`, `EntityFactory`,
`EntitiesLogic`; `Scene.LateInit` builds the controllers and the `Map`, sets up one team per configured
anthill, then raises `MapCreatedEvent`. `Tick` advances `Time`, then `TaskScheduler`, then `Scene` on
`Time.LogicDeltaTime` (real delta × a multiplier, never `UnityEngine.Time`), then drains the `EventBus`
main-thread queue. `Scene` is also the query surface other systems and states use for spatial questions —
`CanPlaceArea`, `IsCoordinateInsideMap`, `GetEmptyAnthillPoint`, `GetSpawnTeamControllerArchitecture(teamId)`.

**Controllers own gameplay rules through a request → accepted event handshake.** Under
`GameLogic/Controllers/`, each `…ControllerArchitecture` is a plain `IDisposable` (not a service) that
subscribes to a `…RequestEvent`, validates it, and raises the matching `…RequestAcceptedEvent`; the system that
actually mutates state (`Scene` swapping tiles, `EntityFactory` creating entities) listens only to the
*accepted* event. So nothing spawns or edits terrain by calling a method — raise the request and let the
controller decide. Spawning an ant is `SpawnRequestEvent<T>` → controller → `SpawnRequestAcceptedEvent<T>` →
`EntityFactory`. Teams are the unit of ownership: `Scene` holds one `SpawnTeamControllerArchitecture` per team
index, which owns that team's `SpawnAnthillControllerArchitecture` (random 5×5 placement with margin, via
`ModifyTerrainEXRequestEvent` whose `tileExceptions` are x-major footprint indices left untouched) and
`SpawnQueenControllerArchitecture` (spawns the queen at the anthill centre on `AnthillSpawnedEvent`). Team
controllers filter every broadcast event by `teamId`, since all teams hear all events. The team ID travels
through tiles (`SwapTile(…, teamId)`), entities (`Entity.teamID`) and the spawn/created events.

- **`Map`** builds tile definitions from the `Tile Types` sheet, raises `GridCreatedEvent`, fills a `Tile[,]`
  with the sheet's `Is Default` tile raising a `TileCreatedEvent` per cell, then raises `MapCreatedEvent`.
  `Tile` is a struct holding only a `TileTypeId` hash; `SwapTile` mutates a cell and raises `TileModifiedEvent`.
  `instances` tracks coordinates per tile ID, which is what makes "is unique / already placed" queries cheap.
- **`Coordinate`** is a readonly min/max rectangle (`IsSingleCoordinate` when it collapses to one cell) that
  enumerates `Points`, `Inner`, etc.; **`Point`** is a readonly int pair. Both are engine-free stand-ins for
  `Vector2Int` — use them in Architecture, and convert at the View boundary via `GameScene.CoordinateToWorld`.
- **`Entity`** is the base for everything spawned: `ID`, `Coordinate`, `teamID`, `Init`/`LateInit`/`Tick`,
  and `Move`, which mutates the coordinate then raises `EntityMovedEvent`. `EntitiesLogic.Tick` ticks every
  registered entity. Each ant type subclasses it and owns its pattern instance, built in `Init` and advanced in
  `Tick`. `QueenAnt` is the reference: an `Fsm` over `internal enum States` / `Flags`, states under
  `QueenAnt/States/`, delta time cached in `Tick` and exposed to states through a `Func<float>`.
- **`EntityFactory` is reflection-driven.** At construction it scans the executing assembly for concrete
  `Entity` subclasses, caches each one's **non-public** `(uint id, Coordinate, int teamId)` constructor, and
  subscribes itself to `SpawnRequestAcceptedEvent<ThatType>`. `CreateInstance<T>` assigns the next ID, invokes
  the cached constructor, applies the blueprint, `Init`s, registers via `EntityRegistry`'s private `Register`,
  raises `EntityCreatedEvent<T>` **once per level of the inheritance chain, base-first**, then calls
  `LateInit`. A view subscribed to `EntityCreatedEvent<Entity>` therefore sees every entity regardless of
  concrete type. A new entity type needs no factory edit — only a protected `(uint, Coordinate, int)`
  constructor (any other signature is silently skipped, then `CreateInstance` throws `MissingMethodException`),
  a blueprint row, a spawn controller that raises its accepted event, and a `[ViewOf]` view.
- **`GameConsole`** (`Log`/`Warning`/`Error`) raises console events instead of calling `Debug.Log`; that is how
  the engine-free assemblies log. `ConsoleView` is the only thing that forwards them to `UnityEngine.Debug`.
  Use `GameConsole` inside `Architecture/` and `ToolBox/`.

### `Parcial.View` — the Unity mirror

`GameplayView` is the single `MonoBehaviour` entry point: `Awake` calls `ViewArchitectureMap.Init()`, constructs
`Gameplay` with the xlsx and map-config paths, registers `PrefabsRegistryView` and a `GameScene`; `Start` runs
`Init` then `LateInit` on both halves; `Update` ticks logic then view; `OnDisable` disposes.

The two halves are paired by attribute, not by name: a view class carries `[ViewOf(typeof(ArchitectureType))]`
and `ViewArchitectureMap` reflects over the calling assembly at startup to build both directions of the map.
`EntityFactoryView.OnEntityCreated` then resolves `ViewArchitectureMap.ViewOf(entity.GetType())` to know which
`ViewComponent` to attach — so **a new entity type needs a matching `[ViewOf]` view class or the lookup throws**.

`ViewComponent : MonoBehaviour` mirrors `IInitable`/`ITickable`/`IDisposable`, with an
`Init(params object[])` overload for constructed instances. Views are created through
`GameScene.AddSceneComponent<T>(name, parent, prefab)`, which makes (or instantiates) the GameObject, adds the
component, and registers it with the nearest ancestor `Container` — a per-instance-ID GameObject registry used
for teardown (`MapView.DestroyTile` looks instances up there). Sprites are layered with `GameScene`'s
`MAP_DRAWING_ORDER` / `ENTITIES_DRAWING_ORDER`.

`PrefabsRegistryView` resolves prefabs through blueprints, not `[SerializeField]`: the `*View` sheets map an
`Architecture ID` to a `Prefab resource path` under `Resources/`, loaded lazily and cached, falling back to
`Resources/Prefabs/MissingPrefab` with a `GameConsole.Warning`. `MapView` owns the Unity `Grid`, builds tiles
in response to the map events, and converts `Coordinate`/`Point` to world positions.
`GridView` is an earlier, mostly commented-out attempt at the same job — `MapView` is the live one.

### Legacy coursework in `Assets/`

`Agent.cs` (HSM creeper: Patrol → Chase → Explode), `AgentBT.cs` (gold-mining worker), `Traveler.cs` +
`GraphView.cs` + `Vector2IntGraph.cs` (pathfinding demo), `Flocking/` (boids with Alignment/Cohesion/Separation/
Direction weights — the starting point for checklist item 52) and the states/nodes under `Scripts/`. They now
consume `KuznickiAgents.*` from the DLL. `AgentHSM.cs` and `EnemyAgent.cs` are commented-out scaffolds.
`crocante.txt` at the git root is a plain-text snapshot of the old generic `FSM<TState, TTrigger>` API; it is
outside `Assets/`, does not compile, and is reference only.

## Conventions and gotchas

- **`*.csproj` and `*.sln` are gitignored** (`.gitignore:35,37`), which includes
  `KuznickiAgents/KuznickiAgents.sln` and `KuznickiAgents/KuznickiAgents/KuznickiAgents.csproj` — a fresh clone
  gets the `.cs` files and the prebuilt DLL but cannot rebuild the library. Since "the pattern code lives in its
  own assembly/DLL" is graded, either force-add those two files or be ready to recreate them (netstandard2.1,
  nothing else needed). The `.cs.meta` files still sitting in `KuznickiAgents/` are leftovers from when the code
  lived under `Assets/`; harmless, and Unity does not read them there.
- **Positional `params object[]` is the recurring hazard.** It is how events assign, how blueprints bind, and
  how state parameters unpack — none of it is compile-checked. A mismatch shows up at runtime as an
  `InvalidCastException` or `IndexOutOfRangeException` inside `Assign`, far from the raise site. When adding
  a field to an event, grep every `Raise<ThatEvent>(` call and update them together (the `teamId` plumbing
  touched `SpawnRequestEvent`, `SpawnRequestAcceptedEvent` and `EntityCreatedEvent` this way).
- **Events raised from multi-threadable buckets reach plain subscribers on worker threads.** The queen's
  `StandByState` calls `GameConsole.Log` from one, so any `Subscribe` handler can run off the Unity thread.
  View handlers that touch Unity objects must use `SubscribeOnlyMainThread` (as `MapView` does);
  `ConsoleView` gets away with plain `Subscribe` only because `Debug.Log` is thread-safe.
- **Three unrelated node types** — `KuznickiAgents.Pathfinding.PathfindingNode` (via `IPathfindingNode`),
  `KuznickiAgents.StatePattern.Node` (HSM hierarchy) and `KuznickiAgents.StatePattern.BehaviourTree.BTNode`.
  The BT one was renamed to `BTNode` to break the old collision; check which namespace a `Node` refers to.
  Separately, `Grid` is ambiguous inside View code (`Parcial.Architecture.GameLogic.Map.Grid` vs
  `UnityEngine.Grid`), and `Parcial.Architecture.Time.Time` shadows `UnityEngine.Time`.
- **Classes are not one-per-file.** `ChaseState` lives in `PatrolState.cs`; every BT action and condition node
  (`IsTargetClose`, `MoveToTransformAction`, `GetGoldAction`, …) lives in `CanSeePlayerCondition.cs`; `MapConfig`
  and `MapConfigIO` live in `MapConfigurator.cs`. Search by symbol, not by filename.
- Identifiers mix Spanish and English (`ayuntamiento`, `hormigueros`, `"Te vi"`), and several typos are baked
  into public API names: `AddMultiTrheadableBehaviour`, `DijstraPathfinder`, `AcumulativeCost`, `IsPersistance`,
  the `ArchitectureAssenbly` asmdef, `IResetteable` (an unused duplicate of `IResettable`). Renaming them is a
  breaking change across call sites — match the existing spelling unless deliberately refactoring.
- Indentation is inconsistent even within a folder (tabs in `ToolBox/Blueprint/`, spaces in
  `Parcial/Architecture/Scene.cs` and `Parcial/View/`). Follow the file you are editing.
- `.cs` files under `Assets/` must keep their sibling `.meta`; when adding or deleting scripts outside the
  editor, add or delete the `.meta` alongside so scene and prefab references survive.
- The editor regenerates `Assembly-CSharp.csproj` and the `.sln` inside `InteligenciaArtificial/` — never
  hand-edit them.
