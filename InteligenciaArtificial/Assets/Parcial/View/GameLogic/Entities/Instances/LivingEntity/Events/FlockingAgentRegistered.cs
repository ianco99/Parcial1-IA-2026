using ianco99.ToolBox.Events;

namespace Parcial.View.GameLogic.Entities.Instances.LivingEntity.Events
{
	public struct FlockingAgentRegisteredEvent : IEvent
	{
		public uint id;
		public void Assign(params object[] parameters)
		{
			id = (uint)parameters[0];
		}

		public void Reset()
		{
			id = default(uint);
		}
	}
}