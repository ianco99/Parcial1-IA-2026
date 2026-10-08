using ianco99.ToolBox.Services;
using KuznickiAgents.StatePattern;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.WarriorAnt.States
{
	public class MoveState : State
	{
		EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

		private Point? endPoint;

		private int arrivalDistance;
		private string arrivedFlag;

		public MoveState(params object[] parameters)
		{
			arrivalDistance = (int)parameters[0];
			arrivedFlag = (string)parameters[1];
		}

		public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
		{
			BehaviourActions actions = new BehaviourActions();

			Point? destination = (Point?)parameters[0];
			uint ownerId = (uint)parameters[1];

			actions.AddMainThreadableBehaviour(0,
				() =>
				{
					Instances.WarriorAnt.WarriorAnt.Log(ownerId, $"MoveState enter: destination={Describe(destination)}, me={DescribeEntity(ownerId)}");

					endPoint = null;

					if (!destination.HasValue)
					{
						Instances.WarriorAnt.WarriorAnt.Log(ownerId, "MoveState enter: no destination -> no path");
						return;
					}

					FindPathTo(ownerId, destination.Value);
				});

			return actions;
		}

		public override BehaviourActions GetTickBehaviours(params object[] parameters)
		{
			BehaviourActions actions = new BehaviourActions();

			Point? destination = (Point?)parameters[0];
			uint ownerId = (uint)parameters[1];

			actions.AddMainThreadableBehaviour(0, () =>
			{
				Instances.WarriorAnt.WarriorAnt.Log(ownerId, $"MoveState tick: destination={Describe(destination)}, me={DescribeEntity(ownerId)}");

				if (!destination.HasValue)
				{
					Instances.WarriorAnt.WarriorAnt.Log(ownerId, "MoveState: no destination -> standing still");
					return;
				}

				if (!endPoint.HasValue || endPoint.Value != destination.Value)
				{
					Instances.WarriorAnt.WarriorAnt.Log(ownerId, $"MoveState: destination changed from {Describe(endPoint)} -> FindPath again");
					FindPathTo(ownerId, destination.Value);
				}
			});

			actions.AddMainThreadableBehaviour(1,
				() =>
				{
					if (!destination.HasValue || HasArrived(ownerId, destination.Value))
						return;

					bool moved = ((Instances.WarriorAnt.WarriorAnt)EntityRegistry[ownerId]).MoveAlongPath();

					Instances.WarriorAnt.WarriorAnt.Log(ownerId, $"MoveState: MoveAlongPath -> {(moved ? "moved" : "did not move (no path or still on move cooldown)")}, me={DescribeEntity(ownerId)}");
				});

			actions.AddMainThreadableBehaviour(2, () =>
			{
				if (!destination.HasValue || !HasArrived(ownerId, destination.Value))
					return;

				if (string.IsNullOrEmpty(arrivedFlag))
				{
					Instances.WarriorAnt.WarriorAnt.Log(ownerId, $"MoveState: arrived at {Describe(destination)}");
					return;
				}

				Instances.WarriorAnt.WarriorAnt.Log(ownerId, $"MoveState: arrived at {Describe(destination)} -> raising {arrivedFlag}");
				OnTrigger?.Invoke(arrivedFlag);
			});

			return actions;
		}

		public override BehaviourActions GetOnExitBehaviours(params object[] parameters)
		{
			if (Instances.WarriorAnt.WarriorAnt.LogsEnabled)
				GameConsole.Log("[WarriorAnt HSM] MoveState exit");

			return new BehaviourActions();
		}

		private string DescribeEntity(uint entityID)
		{
			if (!EntityRegistry.Contains(entityID))
				return "not registered";

			Entity entity = EntityRegistry[entityID];
			return $"{entity.GetType().Name} at ({entity.Coordinate.Origin.x}, {entity.Coordinate.Origin.y})";
		}

		private static string Describe(Point? point) => point.HasValue ? $"({point.Value.x}, {point.Value.y})" : "none";

		private void FindPathTo(uint ownerID, Point destination)
		{
			endPoint = destination;

			bool found = ((Instances.WarriorAnt.WarriorAnt)EntityRegistry[ownerID]).FindPath(destination);

			Instances.WarriorAnt.WarriorAnt.Log(ownerID, $"MoveState: FindPath to ({destination.x}, {destination.y}) -> {(found ? "path found" : "NO path")}");
		}

		private bool HasArrived(uint ownerID, Point destination)
		{
			Point mine = EntityRegistry[ownerID].Coordinate.Origin;

			return System.Math.Abs(mine.x - destination.x) <= arrivalDistance &&
			       System.Math.Abs(mine.y - destination.y) <= arrivalDistance;
		}
	}
}