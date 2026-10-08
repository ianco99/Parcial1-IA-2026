using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture.GameLogic.Entities.Events;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.Food
{
	public class Food : Entity
	{
		public const int MAX_AMOUNT = 20;
		int currentAmount = 0;

		public int CurrentAmount => currentAmount;

		public bool IsStored => teamID != UNASSIGNED_ENTITY_TEAM_ID;

		EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

		protected Food(uint ID, Coordinate coordinate, int teamId) : base(ID, coordinate, teamId)
		{
			currentAmount = teamId == UNASSIGNED_ENTITY_TEAM_ID ? MAX_AMOUNT : 1;
		}

		public void Stack()
		{
			currentAmount++;

			EventBus.Raise<FoodAmountChangedEvent>(ID, currentAmount);
		}

		public void SetAmount(int amount)
		{
			currentAmount = amount;

			EventBus.Raise<FoodAmountChangedEvent>(ID, currentAmount);
		}

		public void GetCollected()
		{
			currentAmount--;

			EventBus.Raise<FoodAmountChangedEvent>(ID, currentAmount);

			if (currentAmount <= 0)
			{
				EntityRegistry.UnRegister(this);
			}
		}
	}
}