using System.Collections.Generic;
using ianco99.ToolBox.Blueprints;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture.GameLogic.Map.Events;
using Parcial.Architecture.Math;
using Parcial.View.Resources;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Parcial.View.Scene
{
    internal sealed class MapView : ViewComponent
    {
        private BlueprintRegistry BlueprintRegiistry => ServiceProvider.Instance.GetService<BlueprintRegistry>();
        private BlueprintBinder BlueprintBinder => ServiceProvider.Instance.GetService<BlueprintBinder>();

        private PrefabsRegistryView PrefabsRegistryView => ServiceProvider.Instance.GetService<PrefabsRegistryView>();

        private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        private Container container;

        private Grid grid;

        private Dictionary<(int x, int y), int> instanceHashPerCoordinate;

        private Dictionary<int, (string ID, string path)> pathToTilePrefabByIDHash;

        public override void Init()
        {
            base.Init();
            instanceHashPerCoordinate = new Dictionary<(int x, int y), int>();
            grid = gameObject.AddComponent<Grid>();

            
            grid.cellSize = Vector3.back;
            
            container = GameScene.GetContainer(this);

            LoadTilePrefabPaths();

            EventBus.SubscribeOnlyMainThread<GridCreatedEvent>(OnGridCreated);
            EventBus.SubscribeOnlyMainThread<TileCreatedEvent>(OnTileCreated);
            EventBus.SubscribeOnlyMainThread<MapCreatedEvent>(OnMapCreated);
            EventBus.SubscribeOnlyMainThread<TileModifiedEvent>(OnTileModified);
        }

        private void OnGridCreated(in GridCreatedEvent gridCreatedEvent)
        {
            //grid.cellGap = Vector3.one;
            //grid.cellLayout = GridLayout.CellLayout.Isometric;
            grid.cellSize = new Vector3(gridCreatedEvent.tileSizeX, gridCreatedEvent.tileSizeY, 1.0f);
        }

        private void LoadTilePrefabPaths()
        {
            pathToTilePrefabByIDHash = new Dictionary<int, (string, string)>();

            foreach (string blueprint in BlueprintRegiistry.BlueprintsOf(TableNamesView.TILES_VIEW_TABLE_NAME))
            {
                object tileViewData = new TileViewData();
                BlueprintBinder.Apply(ref tileViewData, TableNamesView.TILES_VIEW_TABLE_NAME, blueprint);
                pathToTilePrefabByIDHash.Add(((TileViewData)tileViewData).ArchitectureIHHash, (((TileViewData)tileViewData).architectureID, ((TileViewData)tileViewData).prefabPath));
            }
        }

        private void OnMapCreated(in MapCreatedEvent mapCreatedEvent)
        {
            EventBus.UnsubscribeOnlyMainThread<TileCreatedEvent>(OnTileCreated);
        }

        private void OnTileCreated(in TileCreatedEvent tileCreatedEvent)
        {
            CreateTile(tileCreatedEvent.tileId, tileCreatedEvent.xCoord, tileCreatedEvent.yCoord, tileCreatedEvent.teamId);
        }
        private void OnTileModified(in TileModifiedEvent tileModifiedEvent)
        {
            DestroyTile(tileModifiedEvent.xCoord, tileModifiedEvent.yCoord);
            CreateTile(tileModifiedEvent.newTileId, tileModifiedEvent.xCoord, tileModifiedEvent.yCoord, tileModifiedEvent.newTeamId);
        }

        private void DestroyTile(int coordX, int coordY) 
        {
            int hashToDestroy = instanceHashPerCoordinate[(coordX, coordY)];
            Destroy(container[hashToDestroy]);
            instanceHashPerCoordinate.Remove((coordX, coordY));
        }

        private void CreateTile(int tileId, int coordX, int coordY, int teamId)
        {
            GameObject tileToSpawn = PrefabsRegistryView.Get(TableNamesView.TILES_VIEW_TABLE_NAME, pathToTilePrefabByIDHash[tileId].ID);
            GameObject tileInstance = Instantiate(tileToSpawn,
                grid.CellToLocal(new Vector3Int(coordX, coordY, 0))
                + new Vector3(grid.cellSize.x * 0.5f, grid.cellSize.y * 0.5f, 0.0f),
                Quaternion.identity, grid.gameObject.transform);
            instanceHashPerCoordinate.Add((coordX, coordY), tileInstance.GetInstanceID());

            tileInstance.transform.localScale = new Vector3(grid.cellSize.x, grid.cellSize.y, 1.0f);
            container.Register(tileInstance);

            if (tileInstance.TryGetComponent(out SpriteRenderer sprite))
            {
                sprite.sortingOrder = GameScene.MAP_DRAWING_ORDER;
                sprite.color = TeamColorsView.GetColor(teamId);
            }
        }

        public override void LateInit()
        {
            base.LateInit();
        }

        public Vector3 CoordinateToGrid(Coordinate coordinate)
        {
            return PointToGrid(coordinate.Origin);
        }

        public Vector3 PointToGrid(Point point) 
        {
            Vector3Int coord = new Vector3Int(point.x, point.y, 0);
            Vector3 output = grid.GetCellCenterWorld(coord);
            output.z = 0.0f;
            return output;
        }

        public Point GetCoordinateAsPointInGrid(CameraView cameraView, Vector3 coordinate)
        {
            Vector3 mouseScreePosition = coordinate;
            Vector3 mouseWorldPosition = cameraView.GameCamera.ScreenToWorldPoint(mouseScreePosition);
            mouseScreePosition.z = 0.0f;

            Vector3Int cellCoordinate = grid.WorldToCell(mouseWorldPosition);
            return new Point(cellCoordinate.x, cellCoordinate.y);
        }

        public Point GetMouseCoordinateAsPointInGrid(CameraView cameraView)
        {
            return GetCoordinateAsPointInGrid(cameraView, Mouse.current.position.ReadValue());
        }

        public Vector3 GetMouseWorldPosition(CameraView cameraView)
        {
            Vector3 mouseWorldPosition = cameraView.GameCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mouseWorldPosition.z = 0.0f;
            return mouseWorldPosition;
        }

        public override void Dispose()
        {
            base.Dispose();
            EventBus.UnsubscribeOnlyMainThread<GridCreatedEvent>(OnGridCreated);
            EventBus.UnsubscribeOnlyMainThread<TileCreatedEvent>(OnTileCreated);
            EventBus.UnsubscribeOnlyMainThread<MapCreatedEvent>(OnMapCreated);
            EventBus.UnsubscribeOnlyMainThread<TileModifiedEvent>(OnTileModified);
        }

        public Vector2 CellSize => grid.cellSize;

        struct TileViewData
        {
            [BlueprintParameter("Architecture ID")] public string architectureID;
            [BlueprintParameter("Prefab resource path")] public string prefabPath;

            public int ArchitectureIHHash => architectureID.GetHashCode();
        }
    }
}