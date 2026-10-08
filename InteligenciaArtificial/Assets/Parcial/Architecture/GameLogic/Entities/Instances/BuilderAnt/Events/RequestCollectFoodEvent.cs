using ianco99.ToolBox.Events;

namespace Parcial.Architecture.GameLogic.Entities.Events
{
	public struct RequestCollectFoodEvent : IEvent
	{
		public uint collector;
		public int collectorTeamId;
		public uint foodID;
		public void Assign(params object[] parameters)
		{
			this.collector = (uint)parameters[0];
			this.collectorTeamId = (int)parameters[1];
			this.foodID = (uint)parameters[2];
		}

		public void Reset()
		{
			this.collector = 0;
			this.collectorTeamId = 0;
			this.foodID = 0;
		}
	}
}