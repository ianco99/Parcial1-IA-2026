using KuznickiAgents.StatePattern.BehaviourTree;
using UnityEngine;

public sealed class CanSeePlayerCondition : ConditionalNode
{
	private readonly Transform self;
	private readonly Transform player;
	private readonly float viewRange;

	public CanSeePlayerCondition(Transform self, Transform player, float viewRange, BTNode child)
		: base(child)
	{
		this.self = self;
		this.player = player;
		this.viewRange = viewRange;
	}

	protected override bool Condition()
	{
		if (player == null) return false;
		return Vector3.Distance(self.position, player.position) <= viewRange;
	}
}

public sealed class IsTargetClose : ConditionalNode
{
	private readonly Transform self;
	private readonly Transform target;
	private readonly float attackRange;

	public IsTargetClose(Transform self, Transform target, float attackRange, BTNode child)
		: base(child)
	{
		this.self = self;
		this.target = target;
		this.attackRange = attackRange;
	}

	protected override bool Condition()
	{
		if (target == null) return false;
		return Vector3.Distance(self.position, target.position) <= attackRange;
	}
}

public sealed class PlayerHasGoldCondition : ConditionalNode
{
	private readonly AgentBT self;
	private readonly float goldNeeded;

	public PlayerHasGoldCondition(AgentBT self, float goldNeeded, BTNode child)
		: base(child)
	{
		this.self = self;
		this.goldNeeded = goldNeeded;
	}

	protected override bool Condition()
	{
		if (self == null) return false;
		return self.currentGold >= goldNeeded;
	}
}

public sealed class IsCurrentLocationCondition : ConditionalNode
{
	AgentBT agent;
	private int location;

	public IsCurrentLocationCondition(AgentBT agent, int location, BTNode child) : base(child)
	{
		this.agent = agent;
		this.location = location;
	}

	protected override bool Condition()
	{
		if (agent == null) return false;
		return agent.currentLocationID == location;
	}
}

public sealed class LeaveGoldAction : ActionNode
{
	AgentBT agent;

	public LeaveGoldAction(AgentBT agent)
	{
		this.agent = agent;
	}

	public override NodeState Run(Blackboard blackboard)
	{
		agent.currentGold = 0;
		return NodeState.Success;
	}
}

public sealed class GetGoldAction : ActionNode
{
	AgentBT agent;

	public GetGoldAction(AgentBT agent)
	{
		this.agent = agent;
	}

	public override NodeState Run(Blackboard blackboard)
	{
		agent.currentGold = 50;
		return NodeState.Success;
	}
}

public sealed class ChangeCurrentLocationAction : ActionNode
{
	private readonly AgentBT agent;
	private readonly int location;

	public ChangeCurrentLocationAction(AgentBT agent, int newLocation)
	{
		this.agent = agent;
		this.location = newLocation;
	}

	public override NodeState Run(Blackboard blackboard)
	{
		agent.currentLocationID = location;
		return NodeState.Success;
	}
}

public sealed class MoveToTransformAction : ActionNode
{
	private readonly Transform self;
	private readonly Transform player;
	private readonly float moveSpeed;

	public MoveToTransformAction(Transform self, Transform player, float moveSpeed)
	{
		this.self = self;
		this.player = player;
		this.moveSpeed = moveSpeed;
	}

	public override NodeState Run(Blackboard blackboard)
	{
		if (player == null) return NodeState.Failure;

		Vector3 targetPos = new Vector3(player.position.x, self.position.y, player.position.z);
		self.position = Vector3.MoveTowards(self.position, targetPos, moveSpeed * Time.deltaTime);

		Vector3 lookDir = targetPos - self.position;
		if (lookDir.sqrMagnitude > 0.001f)
			self.rotation = Quaternion.LookRotation(lookDir);

		return NodeState.Running;
	}
}

public sealed class AttackAction : ActionNode
{
	private readonly float cooldown;
	private float lastAttackTime = 0;

	public AttackAction(float cooldown = 1f)
	{
		this.cooldown = cooldown;
	}

	public override NodeState Run(Blackboard blackboard)
	{
		if (Time.time - lastAttackTime >= cooldown)
		{
			lastAttackTime = Time.time;
			Debug.Log("Ataque al Player");
		}

		return NodeState.Running;
	}
}

public sealed class RotateIdleAction : ActionNode
{
	private readonly Transform self;
	private readonly float rotateSpeed;

	public RotateIdleAction(Transform self, float rotateSpeed)
	{
		this.self = self;
		this.rotateSpeed = rotateSpeed;
	}

	public override NodeState Run(Blackboard blackboard)
	{
		self.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
		return NodeState.Running;
	}
}