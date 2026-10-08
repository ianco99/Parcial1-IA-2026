using ianco99.ToolBox.Services;
using KuznickiAgents.StatePattern.BehaviourTree;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.Nodes
{
    public class MoveToPointAction : ActionNode
    {
        EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();


        public override NodeState Run(Blackboard blackboard)
        {
            blackboard.TryGetData(BuilderAnt.ENTITY_ID_KEY, out object data);

            uint id = (uint)data;

            LivingEntity.LivingEntity entity = (LivingEntity.LivingEntity)EntityRegistry[id];

            if (!entity.HasPath)
            {
                BuilderAnt.Log(blackboard, $"[BuilderAnt BT] MoveToPointAction: ant {id} has no path to follow -> Failure");
                return NodeState.Failure;
            }

            if (entity.MoveAlongPath())
            {
                Point antPosition = entity.Coordinate.Origin;
                BuilderAnt.Log(blackboard, $"[BuilderAnt BT] MoveToPointAction: ant {id} stepped to ({antPosition.x}, {antPosition.y})");
            }

            if (entity.HasPath)
                return NodeState.Running;

            BuilderAnt.Log(blackboard, $"[BuilderAnt BT] MoveToPointAction: ant {id} reached the end of its path -> Success");
            return NodeState.Success;
        }
    }
}
