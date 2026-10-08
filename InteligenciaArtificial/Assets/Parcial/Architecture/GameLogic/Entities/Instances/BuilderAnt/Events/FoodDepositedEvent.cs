using ianco99.ToolBox.Events;

namespace Parcial.Architecture.GameLogic.Entities.Events
{
	public struct FoodDepositedEvent : IEvent
	{
		public int teamId;
		public int amount;
		public void Assign(params object[] parameters)
		{
			this.teamId = (int)parameters[0];
			this.amount = (int)parameters[1];
		}

		public void Reset()
		{
			this.teamId = 0;
			this.amount = 0;
		}
	}
}
