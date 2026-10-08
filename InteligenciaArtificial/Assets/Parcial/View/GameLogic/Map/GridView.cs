using ianco99.ToolBox.DataFlow;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture.GameLogic.Map.Events;
using Parcial.View.Scene;
using UnityEngine;

namespace Parcial.View.GameLogic.Map
{
    public sealed class GridView : ViewComponent, IInitable

    {
        private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        private Grid grid;        
        [SerializeField] private GameObject tilePrefab;
        

        public override void Init()
        {
            base.Init();
        }

        public override void LateInit()
        {
            base.LateInit();
            
            //EventBus.Subscribe<TileCreatedEvent>(OnTileCreated);
            //EventBus.Subscribe<GridCreatedEvent>(OnGridCreatedEvent);
        }

        private void OnGridCreatedEvent(in GridCreatedEvent gridCreatedEvent)
        {
            grid = gameObject.AddComponent<Grid>();

            grid.cellGap = Vector3.one;
            grid.cellLayout = GridLayout.CellLayout.Isometric;
            grid.cellSize = new Vector3(gridCreatedEvent.tileSizeX, gridCreatedEvent.tileSizeY, 1.0f);
        }

        private void CreateBaseTiles(int sizeX, int sizeY, int tileSizeX)
        {
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    grid.CellToWorld(new Vector3Int(sizeX, sizeY, 1));
                }
            }
        }

        private void OnTileCreated(in TileCreatedEvent tileCreatedEvent)
        {
            
        }
    }
}