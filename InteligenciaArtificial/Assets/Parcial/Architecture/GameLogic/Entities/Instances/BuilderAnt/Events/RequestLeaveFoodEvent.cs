using ianco99.ToolBox.Events;

namespace Parcial.Architecture.GameLogic.Entities.Events
{
	public struct RequestLeaveFoodEvent : IEvent
	{
		public uint depositor;
		public int depositorTeamId;
		public uint queenID;
		public void Assign(params object[] parameters)
		{
			this.depositor = (uint)parameters[0];
			this.depositorTeamId = (int)parameters[1];
			this.queenID = (uint)parameters[2];
		}

		public void Reset()
		{
			this.depositor = 0;
			this.depositorTeamId = 0;
			this.queenID = 0;
		}
	}
}
