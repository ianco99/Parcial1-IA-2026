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

		private Entities.QueenAnt queenAnt;
		
		private float cooldown = Entities.QueenAnt.EGG_COOLDOWN;
		private float elapsed = 0.0f;

		private Point availableEggPoint;
		private bool hasFood;

		private bool CanSpawnEgg => elapsed >= cooldown;

		private int teamID;

		public StandByState(params object[] parameters)
		{
			teamID = (int)parameters[0];
			queenAnt = (Entities.QueenAnt)parameters[1];
		}
		

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

			Func<float> deltaTimeAction = (Func<float>)parameters[0];

			behaviourActions.AddMultiThreadableBehaviour(0, () =>
			{
				if (deltaTimeAction != null)
					elapsed += deltaTimeAction.Invoke();
			});

			behaviourActions.AddMultiThreadableBehaviour(1, () =>
			{
				if (CanSpawnEgg)
				{
					FindEggSpot();
				}
			});

			behaviourActions.AddMultiThreadableBehaviour(1, () =>
			{
				if (CanSpawnEgg)
				{
					FindFood();
				}
			});

			behaviourActions.AddMultiThreadableBehaviour(2, () =>
			{
				if (CanSpawnEgg && availableEggPoint != default && hasFood)
				{
					queenAnt.eggSpawnPoint = availableEggPoint;
					queenAnt.eggHatchType = queenAnt.DecideEggType();
					OnTrigger?.Invoke(Entities.QueenAnt.Flags.OnEggToSpawn.ToString());
				}
			});

			return behaviourActions;
		}

		private void FindFood()
		{
			hasFood = Scene.GetStoredFoodAmount(teamID) >= Entities.QueenAnt.EGG_FOOD_COST;
		}

		private void FindEggSpot()
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