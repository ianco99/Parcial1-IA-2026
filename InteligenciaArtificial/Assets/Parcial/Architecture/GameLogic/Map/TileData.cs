using ianco99.ToolBox.Blueprints;

namespace Parcial.Architecture.GameLogic.Map
{
	public struct TileData
	{
		[BlueprintParameter("Is Structure")] public bool isStructure;
		[BlueprintParameter("Is Walkable")] public bool isWalkable;
		[BlueprintParameter("Is Default")] public bool isDefault;
		[BlueprintParameter("Is Unique")] public bool isUnique;
		[BlueprintParameter("Is Anthill")] public bool isAnthill;
	}
}