using ianco99.ToolBox.Events;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Controllers.ModifyTerrain.Events
{
    public struct ModifyTerrainRequestEvent : IEvent
    {
        public Point origin;
        public Point end;
        public string newTileId;
        public int terrainTeamId;

        public void Assign(params object[] parameters)
        {
            origin = (Point)parameters[0];
            end = (Point)parameters[1];
            newTileId = (string)parameters[2];
            terrainTeamId = (int)parameters[3];
        }

        public void Reset()
        {
            origin = default(Point);
            end = default(Point);
            newTileId = default(string);
            terrainTeamId = default(int);
        }
    }
}