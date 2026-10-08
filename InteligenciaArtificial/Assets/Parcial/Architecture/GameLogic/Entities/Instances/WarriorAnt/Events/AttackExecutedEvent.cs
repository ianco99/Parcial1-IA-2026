using ianco99.ToolBox.Events;

namespace Parcial.Architecture.GameLogic.Entities.Events
{
	public struct AttackExecutedEvent : IEvent
	{
		public uint attackerID;
		public uint targetID;
		public bool attackerWasOnCooldown;
		public void Assign(params object[] parameters)
		{
			this.attackerID = (uint)parameters[0];
			this.targetID = (uint)parameters[1];
			this.attackerWasOnCooldown = (bool)parameters[2];
		}

		public void Reset()
		{
			this.attackerID = 0;
			this.targetID = 0;
			this.attackerWasOnCooldown = false;
		}
	}
}
