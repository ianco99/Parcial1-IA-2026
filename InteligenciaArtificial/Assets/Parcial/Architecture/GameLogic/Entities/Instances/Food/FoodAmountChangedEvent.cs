using ianco99.ToolBox.Events;

namespace Parcial.Architecture.GameLogic.Entities.Events
{
	public struct FoodAmountChangedEvent : IEvent
	{
		public uint foodID;
		public int currentAmount;
		public void Assign(params object[] parameters)
		{
			this.foodID = (uint)parameters[0];
			this.currentAmount = (int)parameters[1];
		}

		public void Reset()
		{
			this.foodID = 0;
			this.currentAmount = 0;
		}
	}
}
