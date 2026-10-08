using KuznickiAgents.StatePattern.BehaviourTree;
using UnityEngine;

//0  ayuntamiento
//1 mina


//Nodo de guardar data en blackboard (en el run haces AddData)
public class AgentBT : MonoBehaviour
{
	public Transform[] mines;
	public Transform ayuntamiento;
	private BehaviourTree behaviourTree;
	public int currentLocationID;
	public int currentMineID;
	public float currentGold;

	private void Awake()
	{
		ChangeCurrentLocationAction changeToMineAction = new ChangeCurrentLocationAction(this, 1);

		LeaveGoldAction leaveGoldAction = new LeaveGoldAction(this);
		PlayerHasGoldCondition enoughGold = new PlayerHasGoldCondition(this, 50.0f, leaveGoldAction);
		SelectorNode goldOrSearch = new SelectorNode(enoughGold, changeToMineAction);

		IsTargetClose rangeOfAyuntamiento =
			new IsTargetClose(transform, ayuntamiento, 1.0f, goldOrSearch);
		MoveToTransformAction approachAyuntamientoAction = new MoveToTransformAction(transform, ayuntamiento, 13.0f);
		SelectorNode moveOrRangeAyuntamiento = new SelectorNode(rangeOfAyuntamiento, approachAyuntamientoAction);

		IsCurrentLocationCondition locAyuntamiento = new IsCurrentLocationCondition(this, 0, moveOrRangeAyuntamiento);

		ChangeCurrentLocationAction changeToAyuntamientoAction = new ChangeCurrentLocationAction(this, 0);
		PlayerHasGoldCondition enoughGold2 = new PlayerHasGoldCondition(this, 50.0f, changeToAyuntamientoAction);

		GetGoldAction getGoldAction = new GetGoldAction(this);

		SelectorNode haveGoldOrMine = new SelectorNode(enoughGold2, getGoldAction);
		IsTargetClose rangeOfMine = new IsTargetClose(transform, mines[currentMineID], 1.0f, haveGoldOrMine);

		MoveToTransformAction approachMineAction = new MoveToTransformAction(transform, mines[currentMineID], 13.0f);
		SelectorNode moveOrRangeMine = new SelectorNode(rangeOfMine, approachMineAction);

		IsCurrentLocationCondition locMine = new IsCurrentLocationCondition(this, 1, moveOrRangeMine);
		

		SelectorNode locSearch = new SelectorNode(locAyuntamiento, locMine);

		SetBlackboardDataAction setBlackboardDataAction = new SetBlackboardDataAction(mines);
		
		SequenceNode initSequence = new SequenceNode(setBlackboardDataAction, locSearch);
		
		RootNode root = new RootNode(locSearch);

		behaviourTree = new global::KuznickiAgents.StatePattern.BehaviourTree.BehaviourTree(root);
	}

	void Update()
	{
		behaviourTree.Tick();
	}
}