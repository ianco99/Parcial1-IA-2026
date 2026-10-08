using System;
using ianco99.ToolBox.Services;
using KuznickiAgents.StatePattern;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.WarriorAnt.States
{
	public class AttackState : State
	{
		EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

		private Func<uint, bool> attackAction;

		private bool awaitingOutcome;

		public AttackState(params object[] parameters)
		{
			attackAction = (Func<uint, bool>)parameters[0];
		}

		public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
		{
			if (Instances.WarriorAnt.WarriorAnt.LogsEnabled)
				GameConsole.Log("[WarriorAnt HSM] AttackState enter");

			BehaviourActions actions = new BehaviourActions();

			actions.AddMainThreadableBehaviour(0, () => { awaitingOutcome = false; });

			return actions;
		}

		public override BehaviourActions GetTickBehaviours(params object[] parameters)
		{
			BehaviourActions actions = new BehaviourActions();

			uint targetID = (uint)parameters[0];
			uint ownerID = (uint)parameters[1];

			actions.AddMainThreadableBehaviour(0, () =>
			{
				Instances.WarriorAnt.WarriorAnt.Log(ownerID, $"AttackState tick: target={targetID}");

				if (!EntityRegistry.Contains(targetID))
				{
					Instances.WarriorAnt.WarriorAnt.Log(ownerID, awaitingOutcome
						? $"AttackState: attack succeeded, target {targetID} is dead -> raising OnTargetLost"
						: $"AttackState: target {targetID} died before being attacked -> raising OnTargetLost");
					OnTrigger?.Invoke(Instances.WarriorAnt.WarriorAnt.Flags.OnTargetLost.ToString());
					return;
				}

				if (awaitingOutcome)
					Instances.WarriorAnt.WarriorAnt.Log(ownerID, $"AttackState: target {targetID} survived the attack -> trying again");

				if (IsTargetAdjacent(ownerID, targetID))
				{
					awaitingOutcome = attackAction != null && attackAction.Invoke(targetID);

					if (awaitingOutcome)
					{
						Instances.WarriorAnt.WarriorAnt.Log(ownerID, $"AttackState: target {targetID} is adjacent -> attacked, checking next tick if it died");
					}
					else
					{
						Instances.WarriorAnt.WarriorAnt.Log(ownerID, $"AttackState: target {targetID} is adjacent but the attack could not be made -> raising OnBackToAnthill");
						OnTrigger?.Invoke(Instances.WarriorAnt.WarriorAnt.Flags.OnBackToAnthill.ToString());
					}
				}
				else
				{
					Point mine = EntityRegistry[ownerID].Coordinate.Origin;
					Point target = EntityRegistry[targetID].Coordinate.Origin;
					Instances.WarriorAnt.WarriorAnt.Log(ownerID, $"AttackState: target {targetID} at ({target.x}, {target.y}) is not adjacent to ({mine.x}, {mine.y}) -> waiting");
				}
			});

			return actions;
		}

		public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
		{
			if (Instances.WarriorAnt.WarriorAnt.LogsEnabled)
				GameConsole.Log("[WarriorAnt HSM] AttackState exit");

			return new BehaviourActions();
		}

		private bool IsTargetAdjacent(uint ownerID, uint targetID)
		{
			Point mine = EntityRegistry[ownerID].Coordinate.Origin;
			Point target = EntityRegistry[targetID].Coordinate.Origin;

			return System.Math.Abs(mine.x - target.x) <= 1 && System.Math.Abs(mine.y - target.y) <= 1;
		}
	}
}
