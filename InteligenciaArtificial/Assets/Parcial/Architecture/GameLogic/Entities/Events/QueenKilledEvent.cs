using ianco99.ToolBox.Events;

namespace Parcial.Architecture.GameLogic.Entities.Events
{
	public struct QueenKilledEvent : IEvent
	{
		public uint queenID;
		public int teamId;
		public void Assign(params object[] parameters)
		{
			this.queenID = (uint)parameters[0];
			this.teamId = (int)parameters[1];
		}

		public void Reset()
		{
			this.queenID = 0;
			this.teamId = 0;
		}
	}
}
