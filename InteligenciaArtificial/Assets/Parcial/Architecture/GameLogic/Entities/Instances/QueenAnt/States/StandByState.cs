using System;
using ianco99.ToolBox.Services;
using KuznickiAgents.StatePattern;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Controllers.Team;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.Instances.QueenAnt.States
{
	internal class StandByState : State
	{
		private Scene Scene => ServiceProvider.Instance.GetService<Scene>();

		private float cooldown = Entities.QueenAnt.EGG_COOLDOWN;
		private float elapsed = 0.0f;

		private Point availableEggPoint;
		private Action<Point> OnEggPointFound;
		private bool hasFood;

		private bool CanSpawnEgg => elapsed >= cooldown;


		public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
		{
			BehaviourActions behaviourActions = new BehaviourActions();

			behaviourActions.AddMainThreadableBehaviour(0, () =>
			{
				elapsed = 0;
			});

			return behaviourActions;
		}

		public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
		{
			return new BehaviourActions();
		}

		public override BehaviourActions GetTickBehaviours(params object[] parameters)
		{
			BehaviourActions behaviourActions = new BehaviourActions();

			float deltaTimeAction = (float)parameters[0];
			int teamID = (int)parameters[1];

			behaviourActions.AddMultiThreadableBehaviour(0, () =>
			{
				if (deltaTimeAction != null)
					elapsed += deltaTimeAction;
			});

			behaviourActions.AddMultiThreadableBehaviour(1, () =>
			{
				if (CanSpawnEgg)
				{
					FindEggSpot(teamID);
				}
			});

			behaviourActions.AddMultiThreadableBehaviour(1, () =>
			{
				if (CanSpawnEgg)
				{
					FindFood(teamID);
				}
			});

			behaviourActions.AddMultiThreadableBehaviour(2, () =>
			{
				if (CanSpawnEgg && availableEggPoint != default && hasFood)
				{
					OnEggPointFound?.Invoke(availableEggPoint);
					OnTrigger?.Invoke(Entities.QueenAnt.Flags.OnEggToSpawn.ToString());
				}
			});

			return behaviourActions;
		}

		private void FindFood(int teamID)
		{
			hasFood = Scene.GetStoredFoodAmount(teamID) >= Entities.QueenAnt.EGG_FOOD_COST;
		}

		private void FindEggSpot(int teamID)
		{
			
			SpawnTeamControllerArchitecture teamController = Scene.GetSpawnTeamControllerArchitecture(teamID);

			Point point = Scene.GetEmptyAnthillPoint(teamController.SpawnAnthillControllerArchitecture.MyAnthill, teamID);
			if (point != default)
			{
				availableEggPoint = point;
			}
			else
			{
				availableEggPoint = default;
			}
		}
	}
}