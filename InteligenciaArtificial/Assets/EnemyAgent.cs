using KuznickiAgents.StatePattern.BehaviourTree;
using UnityEngine;

public class EnemyAgent : MonoBehaviour
{
    [SerializeField] private Transform player;

    [SerializeField] private float viewRange = 8f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotateSpeed = 90f;
    [SerializeField] private float attackCooldown = 1f;

    private BehaviourTree tree;

    private void Awake()
    {
        // Node canSeeBranch = new CanSeePlayerCondition(
        //     transform, player, viewRange,
        //     new SelectorNode(
        //         new IsTargetClose(
        //             transform, player, attackRange,
        //             new AttackAction(attackCooldown)
        //         ),
        //         new MoveToTransformAction(transform, player, moveSpeed)
        //     )
        // );
        //
        // Node root = new SelectorNode(
        //     canSeeBranch,
        //     new RotateIdleAction(transform, rotateSpeed)
        // );
        //
        // tree = new BehaviourTree(new RootNode(root));
    }

    private void Update()
    {
        tree?.Tick();
    }
}
