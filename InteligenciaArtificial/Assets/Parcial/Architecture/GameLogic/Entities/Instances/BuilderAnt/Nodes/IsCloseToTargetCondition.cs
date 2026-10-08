using ianco99.ToolBox.Services;
using KuznickiAgents.StatePattern.BehaviourTree;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.Nodes
{
	public class IsCloseToTargetCondition : ConditionalNode
	{
		private Scene Scene => ServiceProvider.Instance.GetService<Scene>();
		private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

		private uint closerId;
		private uint targetId;
		private string targetIdKey;
		private bool logsEnabled;

		public IsCloseToTargetCondition(BTNode child, uint closerId, string targetIdKey) : base(child)
		{
			this.closerId = closerId;
			this.targetIdKey = targetIdKey;
		}

		public override NodeState Run(Blackboard blackboard)
		{
			if (!blackboard.TryGetData(targetIdKey, out object data))
			{
				BuilderAnt.Log(blackboard, $"[BuilderAnt BT] IsCloseToTargetCondition: no '{targetIdKey}' in blackboard -> Failure");
				return NodeState.Failure;
			}

			targetId = (uint)data;
			logsEnabled = BuilderAnt.LogsEnabled(blackboard);
			return base.Run(blackboard);
		}

		protected override bool Condition()
		{
			if (!EntityRegistry.TryGet(closerId, out Entity closerEntity) ||
			    !EntityRegistry.TryGet(targetId, out Entity targetEntity))
			{
				if (logsEnabled) GameConsole.Log($"[BuilderAnt BT] IsCloseToTargetCondition: ant {closerId} or target {targetId} is not registered -> False");
				return false;
			}

			Point mine = closerEntity.Coordinate.Origin;
			Point target = targetEntity.Coordinate.Origin;

			if (mine == target)
			{
				if (logsEnabled) GameConsole.Log($"[BuilderAnt BT] IsCloseToTargetCondition: ant {closerId} is on target {targetId} -> True");
				return true;
			}

			Point[] points = Scene.GetNeighboringTiles(mine);

			foreach (Point point in points)
			{
				if (point == target)
				{
					if (logsEnabled) GameConsole.Log($"[BuilderAnt BT] IsCloseToTargetCondition: ant {closerId} is next to target {targetId} -> True");
					return true;
				}
			}

			if (logsEnabled) GameConsole.Log($"[BuilderAnt BT] IsCloseToTargetCondition: ant {closerId} is not next to target {targetId} -> False");
			return false;
		}
	}
}