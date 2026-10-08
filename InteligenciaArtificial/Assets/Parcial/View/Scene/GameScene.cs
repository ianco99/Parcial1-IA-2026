using System;
using ianco99.ToolBox.Services;
using Parcial.Architecture.Math;
using Parcial.View.GameLogic.Controllers;
using Parcial.View.GameLogic.Entities.Systems;
using Parcial.View.GameLogic.Voronoi;
using Parcial.View.Resources;
using UnityEngine;

namespace Parcial.View.Scene
{
    internal sealed class GameScene : ViewComponent, IService
    {
        public bool IsPersistance => false;

        public const int MAP_DRAWING_ORDER = 0;
        public const int TEAM_MARKERS_DRAWING_ORDER = 1;
        public const int ENTITIES_DRAWING_ORDER = 2;

        private const string MAP_CONTAINER_NAME = "Map container";
        private const string ENTITIES_CINTAINER_NAME = "Entities container";
        private const string CAMERA_NAME = "Main Game Camera";
        private const string MAP_NAME = "Map";
        private const string FOOD_VORONOI_NAME = "Food Voronoi";
        private const string TERRITORY_VORONOI_NAME = "Territory Voronoi";
        private const string WEIGHTED_VORONOI_NAME = "Weighted Voronoi";
        private const string CONTECT_MENU_VIEW_NAME = "ContextMenu";

        private const string CAMERA_PREFAB_NAME = "GameCamera";
        private const string CONTEXT_MENU_PREFAB_NAME = "ContextMenuContainer";
        private const string CONTEXT_MENU_BUTTON_PREFAB_NAME = "ButtonPrefab";

        private PrefabsRegistryView PrefabsRegistryView => ServiceProvider.Instance.GetService<PrefabsRegistryView>();
        private EntitiesFlocking EntitiesFlocking => ServiceProvider.Instance.GetService<EntitiesFlocking>(); 
        private EntityRegistryView EntityRegistryView => ServiceProvider.Instance.GetService<EntityRegistryView>();
        private ContextMenuView ContextMenuView => ServiceProvider.Instance.GetService<ContextMenuView>();

        private EntityFactoryView entityFactoryView;
        private ControllerViewStrategy controllerViewStrategy;
        private Container mapContainer;
        private Container entitiesContainer;

        internal Container MapContainer => mapContainer;
        internal Container EntitiesContainer => entitiesContainer;

        private MapView mapView;
        private FoodVoronoiView foodVoronoiView;
        private TerritoryVoronoiView territoryVoronoiView;
        private WeightedVoronoiView weightedVoronoiView;
        private CameraView cameraView;
        private EntitiesLogicView entitiesLogicView;

        public override void Init()
        {
            base.Init();

            ServiceProvider.Instance.AddService<EntityRegistryView>(new EntityRegistryView());
            ServiceProvider.Instance.AddService<EntitiesFlocking>(new EntitiesFlocking());
            entityFactoryView = new EntityFactoryView();

            mapContainer = AddSceneComponent<Container>(MAP_CONTAINER_NAME, this.transform);
            mapContainer.Init();
            entitiesContainer = AddSceneComponent<Container>(ENTITIES_CINTAINER_NAME, this.transform);
            entitiesContainer.Init();

            mapView = AddSceneComponent<MapView>(MAP_NAME, MapContainer.transform);
            mapView.Init();

            foodVoronoiView = AddSceneComponent<FoodVoronoiView>(FOOD_VORONOI_NAME, MapContainer.transform);
            foodVoronoiView.Init();

            territoryVoronoiView = AddSceneComponent<TerritoryVoronoiView>(TERRITORY_VORONOI_NAME, MapContainer.transform);
            territoryVoronoiView.Init();

            weightedVoronoiView = AddSceneComponent<WeightedVoronoiView>(WEIGHTED_VORONOI_NAME, MapContainer.transform);
            weightedVoronoiView.Init();

            entitiesLogicView = new EntitiesLogicView();
            entitiesLogicView.Init();
        }

        public override void Init(params object[] parameters)
        {
            base.Init(parameters);
            Canvas canvasView = parameters[0] as Canvas;

            GameObject contextMenuPrefab = PrefabsRegistryView.Get(TableNamesView.UI_VIEW_TABLE_NAME, CONTEXT_MENU_PREFAB_NAME);
            GameObject buttonMenuPrefab = PrefabsRegistryView.Get(TableNamesView.UI_VIEW_TABLE_NAME, CONTEXT_MENU_BUTTON_PREFAB_NAME);

            ContextMenuView contextMenuView = AddSceneComponent<ContextMenuView>(CONTECT_MENU_VIEW_NAME, canvasView.transform, contextMenuPrefab);
            ServiceProvider.Instance.AddService<ContextMenuView>(contextMenuView);
            contextMenuView.Init(buttonMenuPrefab, canvasView);

            GameObject cameraPrefab = PrefabsRegistryView.Get(TableNamesView.CAMERA_VIEW_TABLE_NAME, CAMERA_PREFAB_NAME);

            cameraView = AddSceneComponent<CameraView>(CAMERA_NAME, null, cameraPrefab);
            cameraView.Init(cameraView.GetComponent<Camera>());

        }

        public override void LateInit()
        {
            base.LateInit();
            controllerViewStrategy = new ControllerViewStrategy();
            mapContainer.LateInit();
            entitiesContainer.LateInit();
            mapView.LateInit();
            foodVoronoiView.LateInit();
            territoryVoronoiView.LateInit();
            weightedVoronoiView.LateInit();
            entitiesLogicView.LateInit();
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            controllerViewStrategy?.Tick(deltaTime);

            mapContainer.Tick(deltaTime);
            entitiesContainer.Tick(deltaTime);
            mapView.Tick(deltaTime);
            foodVoronoiView.Tick(deltaTime);
            territoryVoronoiView.Tick(deltaTime);
            weightedVoronoiView.Tick(deltaTime);
            ContextMenuView.Tick(deltaTime);
            cameraView.Tick(deltaTime);
            entitiesLogicView.Tick(deltaTime);
        }

        public Vector3 CoordinateToWorld(Coordinate coordinate)
        {
            return mapView.CoordinateToGrid(coordinate);
        }

        public Vector3 PointToWorld(Point point) 
        {
            return mapView.PointToGrid(point);
        }

        public Point GetMouseGridCoordinate()
        {
            return mapView.GetMouseCoordinateAsPointInGrid(cameraView);
        }

        public Vector3 GetMouseWorldPosition()
        {
            return mapView.GetMouseWorldPosition(cameraView);
        }

        public Point GetGridCoordinate(Vector3 coordinate)
        {
            return mapView.GetCoordinateAsPointInGrid(cameraView, coordinate);
        }

        public Vector2 GetCellSize => mapView.CellSize;

        public override void Dispose()
        {
            base.Dispose();
            controllerViewStrategy?.Dispose();
            entityFactoryView.Dispose();
            mapContainer.Dispose();
            entitiesContainer.Dispose();
            mapView.Dispose();
            foodVoronoiView.Dispose();
            territoryVoronoiView.Dispose();
            weightedVoronoiView.Dispose();
            cameraView.Dispose();
            entitiesLogicView.Dispose();
            EntityRegistryView.Dispose();
            EntitiesFlocking.Dispose();
        }

        public static ComponentType AddSceneComponent<ComponentType>(string name, Transform parent = null, GameObject prefab = null) where ComponentType : ViewComponent
        {
            return AddSceneComponent(typeof(ComponentType), name, parent, prefab) as ComponentType;
        }

        public static ViewComponent AddSceneComponent(Type viewComponentType, string name, Transform parent = null, GameObject prefab = null)
        {
            if (!typeof(ViewComponent).IsAssignableFrom(viewComponentType))
                throw new InvalidOperationException();

            GameObject newSceneObject = prefab == null ? new GameObject() : Instantiate(prefab);
            newSceneObject.name = name;
            if (parent != null)
                newSceneObject.transform.parent = parent;

            ViewComponent viewComponent = newSceneObject.AddComponent(viewComponentType) as ViewComponent;
            Container container = GetContainer(viewComponent);

            if (container != null)
                container.Register(newSceneObject);

            return viewComponent;
        }

        public static Container GetContainer(ViewComponent component)
        {
            Transform parent = component.transform.parent;

            while (parent != null)
            {
                if (parent.gameObject.TryGetComponent(out Container container))
                    return container;
                else
                    parent = parent.parent;
            }

            return null;
        }
    }
}