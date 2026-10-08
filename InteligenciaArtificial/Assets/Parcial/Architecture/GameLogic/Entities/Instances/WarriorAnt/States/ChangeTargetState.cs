using System;
using ianco99.ToolBox.Services;
using KuznickiAgents.StatePattern;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.WarriorAnt.States
{
	public class ChangeTargetState : State
	{
		EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

		private Action<uint> changeTargetAction;
		private Action<int> requestFoodAction;
		private Func<int> storedFoodPointer;

		private float elapsed;

		private (bool, uint) targetBuffer;

		public ChangeTargetState(params object[] parameters)
		{
			changeTargetAction = (Action<uint>)parameters[0];
			requestFoodAction = (Action<int>)parameters[1];
			storedFoodPointer = (Func<int>)parameters[2];
		}

		public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
		{
			if (Instances.WarriorAnt.WarriorAnt.LogsEnabled)
				GameConsole.Log("[WarriorAnt HSM] ChangeTargetState enter");

			BehaviourActions behaviourActions = new BehaviourActions();

			behaviourActions.AddMainThreadableBehaviour(0, () => { elapsed = 0; });

			return behaviourActions;
		}

		public override BehaviourActions GetTickBehaviours(params object[] parameters)
		{
			BehaviourActions behaviourActions = new BehaviourActions();

			float deltaTime = (float)parameters[0];
			int teamFoodAmount = (int)parameters[1];
			int teamID = (int)parameters[2];
			uint ownerID = (uint)parameters[3];

			int availableFood = teamFoodAmount + storedFoodPointer();

			bool engaged = false;

			behaviourActions.AddMainThreadableBehaviour(0, () =>
			{
				elapsed += deltaTime;

				Instances.WarriorAnt.WarriorAnt.Log(ownerID, $"ChangeTargetState tick: elapsed={elapsed}, teamFood={teamFoodAmount}, carried={storedFoodPointer()}, team={teamID}");
			});

			behaviourActions.AddMainThreadableBehaviour(1, () =>
			{
				if (!engaged && availableFood >= 50)
					engaged = TryEngage(ownerID, teamID, Instances.WarriorAnt.WarriorAnt.Targets.QueenAnt, "food >= 50");
			});

			behaviourActions.AddMainThreadableBehaviour(2, () =>
			{
				if (!engaged && elapsed >= 20 && availableFood >= 10)
					engaged = TryEngage(ownerID, teamID, Instances.WarriorAnt.WarriorAnt.Targets.WarriorAnt, "elapsed >= 20 and food >= 10");
			});

			behaviourActions.AddMainThreadableBehaviour(3, () =>
			{
				if (!engaged && elapsed >= 30 && availableFood >= 5)
					engaged = TryEngage(ownerID, teamID, Instances.WarriorAnt.WarriorAnt.Targets.BuilderAnt, "elapsed >= 30 and food >= 5");
			});

			behaviourActions.AddMainThreadableBehaviour(4, () =>
			{
				if (!engaged && elapsed >= 40 && availableFood >= 1)
					engaged = TryEngage(ownerID, teamID, Instances.WarriorAnt.WarriorAnt.Targets.EggAnt, "elapsed >= 40 and food >= 1");
			});

			return behaviourActions;
		}

		public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
		{
			if (Instances.WarriorAnt.WarriorAnt.LogsEnabled)
				GameConsole.Log($"[WarriorAnt HSM] ChangeTargetState exit: elapsed={elapsed}");

			return new BehaviourActions();
		}

		private bool TryEngage(uint ownerID, int teamID, Instances.WarriorAnt.WarriorAnt.Targets target, string reason)
		{
			targetBuffer = SeekTarget(target, teamID);

			if (!targetBuffer.Item1)
			{
				Instances.WarriorAnt.WarriorAnt.Log(ownerID, $"ChangeTargetState: {reason}, no enemy {target} found");
				return false;
			}

			int cost = (int)target;
			int missing = cost - storedFoodPointer();

			Instances.WarriorAnt.WarriorAnt.Log(ownerID, $"ChangeTargetState: {reason}, found enemy {target} {targetBuffer.Item2}, cost={cost}, carried={storedFoodPointer()}");

			if (missing > 0)
				requestFoodAction?.Invoke(missing);

			if (storedFoodPointer() < cost)
			{
				Instances.WarriorAnt.WarriorAnt.Log(ownerID, $"ChangeTargetState: only carrying {storedFoodPointer()} of {cost} after asking the anthill -> staying");
				return false;
			}

			Instances.WarriorAnt.WarriorAnt.Log(ownerID, $"ChangeTargetState: carrying {storedFoodPointer()} -> raising OnFoundTarget on {target} {targetBuffer.Item2}");

			changeTargetAction?.Invoke(targetBuffer.Item2);
			OnTrigger.Invoke(Instances.WarriorAnt.WarriorAnt.Flags.OnFoundTarget.ToString());

			return true;
		}

		private (bool, uint) SeekTarget(Instances.WarriorAnt.WarriorAnt.Targets target, int myTeamID)
		{
			switch (target)
			{
				case Instances.WarriorAnt.WarriorAnt.Targets.EggAnt:
					foreach (EggAnt.EggAnt eggAnt in EntityRegistry.EggAnts)
					{
						if (eggAnt.teamID != myTeamID)
						{
							return (true, eggAnt.ID);
						}
					}

					break;
				case Instances.WarriorAnt.WarriorAnt.Targets.BuilderAnt:
					foreach (BuilderAnt builderAnt in EntityRegistry.BuilderAnts)
					{
						if (builderAnt.teamID != myTeamID)
						{
							return (true, builderAnt.ID);
						}
					}

					break;
				case Instances.WarriorAnt.WarriorAnt.Targets.WarriorAnt:
					foreach (Instances.WarriorAnt.WarriorAnt warriorAnt in EntityRegistry.WarriorAnts)
					{
						if (warriorAnt.teamID != myTeamID)
						{
							return (true, warriorAnt.ID);
						}
					}

					break;
				case Instances.WarriorAnt.WarriorAnt.Targets.QueenAnt:
					foreach (QueenAnt queenAnt in EntityRegistry.QueenAnts)
					{
						if (queenAnt.teamID != myTeamID)
						{
							return (true, queenAnt.ID);
						}
					}

					break;
			}

			return (false, default(uint));
		}
	}
}