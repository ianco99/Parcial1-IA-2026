using ianco99.ToolBox.Events;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.Events
{
	public struct RequestClaimTileEvent : IEvent
	{
		public uint claimer;
		public int claimerTeamId;
		public Point tile;
		public void Assign(params object[] parameters)
		{
			this.claimer = (uint)parameters[0];
			this.claimerTeamId = (int)parameters[1];
			this.tile = (Point)parameters[2];
		}

		public void Reset()
		{
			this.claimer = 0;
			this.claimerTeamId = 0;
			this.tile = default;
		}
	}
}
