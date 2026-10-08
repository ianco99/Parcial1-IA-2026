using ianco99.ToolBox.Events;

namespace Parcial.Architecture.GameLogic.Entities.Events
{
	public struct RequestPickUpFoodEvent : IEvent
	{
		public uint collector;
		public int collectorTeamId;
		public int amount;
		public void Assign(params object[] parameters)
		{
			this.collector = (uint)parameters[0];
			this.collectorTeamId = (int)parameters[1];
			this.amount = (int)parameters[2];
		}

		public void Reset()
		{
			this.collector = 0;
			this.collectorTeamId = 0;
			this.amount = 0;
		}
	}
}
