using ianco99.ToolBox.DataFlow;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture.GameLogic.Entities.Events;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities
{
	public class Entity : IInitable, ITickable
	{
		public const uint UNASSIGNED_ENTITY_ID = 0;
		public const int UNASSIGNED_ENTITY_TEAM_ID = -1;
		public int teamID;
		public uint ID;
		public Coordinate Coordinate;

		private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

		protected Entity(uint ID, Coordinate coordinate, int teamId)
		{
			this.ID = ID;
			this.Coordinate = coordinate;
			teamID = teamId;
		}

		public virtual void Init()
		{
		}

		public virtual void LateInit()
		{
		}

		public virtual void Tick(float deltaTime)
		{
		}

		public void Move(Point offset)
		{
			Coordinate = Coordinate.Move(offset);
			EventBus.Raise<EntityMovedEvent>(ID);
		}
	}
}