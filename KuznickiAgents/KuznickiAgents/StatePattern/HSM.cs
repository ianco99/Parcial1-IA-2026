using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using System.Xml.Linq;

namespace KuznickiAgents.StatePattern
{
    public class Hsm : IDisposable
    {
        private uint lastFlagIDAssigned = 0;

        private readonly NodeIdFactory nodeIdFactory;

        private readonly Dictionary<string, uint> flagsAlias;

        private readonly Dictionary<NodeID, Node> nodes;
        private readonly Dictionary<NodeID, State> states;

        private readonly Dictionary<NodeID, Func<object[]>> enterParameters;
        private readonly Dictionary<NodeID, Func<object[]>> tickParameters;
        private readonly Dictionary<NodeID, Func<object[]>> exitParameters;

        private readonly Dictionary<NodeID, Dictionary<uint, (NodeID destinationNodeID, Action onTransition)>>
            transitions;

        private readonly ParallelOptions parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 32 };

        private NodeID currentNodeID;

        private BehaviourActions GetOnEnterBehaviour(NodeID stateId)
        {
            return states[stateId].GetOnEnterBehaviours(enterParameters[stateId]?.Invoke());
        }

        private BehaviourActions GetOnTickBehaviour(NodeID stateId)
        {
            return states[stateId].GetTickBehaviours(tickParameters[stateId]?.Invoke());
        }

        public Hsm(string[] flagAlias = null)
        {
            this.nodeIdFactory = new NodeIdFactory();

            flagsAlias = new Dictionary<string, uint>();

            if (flagAlias != null)
                foreach (string flag in flagAlias)
                    AddFlagAlias(flag);

            nodes = new Dictionary<NodeID, Node>();
            states = new Dictionary<NodeID, State>();

            //TODO: Improve / Check.

            nodes[NodeID.ROOT_ID] = new Node(NodeID.ROOT_ID, NodeID.ROOT_ID);

            transitions = new Dictionary<NodeID, Dictionary<uint, (NodeID destinationNodeID, Action onTransition)>>();

            enterParameters = new Dictionary<NodeID, Func<object[]>>();
            tickParameters = new Dictionary<NodeID, Func<object[]>>();
            exitParameters = new Dictionary<NodeID, Func<object[]>>();
        }

        public void AddFlagAlias(string flagAlias)
        {
            if (flagsAlias.ContainsKey(flagAlias))
                throw new Exception($"FLAG: {flagAlias} is already registered");

            flagsAlias.Add(flagAlias, ++lastFlagIDAssigned);
        }


        private bool TryGetTransition(Node startingNode, string flag, out (NodeID, Action) transition)
        {
            if (startingNode != null && startingNode.Id != NodeID.ROOT_ID)
            {
                if (transitions.TryGetValue(startingNode.Id, out Dictionary<uint, (NodeID, Action)> nodeTransitions) &&
                    nodeTransitions.TryGetValue(flagsAlias[flag], out transition))
                    return true;

                return TryGetTransition(nodes[startingNode.Parent], flag, out transition);
            }

            transition = default((NodeID, Action));
            return false;
        }

        public void OnFlag(string flag)
        {
            if (!nodes.TryGetValue(currentNodeID, out Node currentNode))
                return;

            if (!TryGetTransition(currentNode, flag, out (NodeID ancestorID, Action transition) transition))
                return;

            ChangeState(transition.ancestorID, transition.transition);
        }

        public NodeID AddState<TState>(int stateIdentifier,
            NodeID parentId = default(NodeID),
            Func<object[]> enterParametersPointer = null,
            Func<object[]> tickParametersPointer = null,
            Func<object[]> exitParametersPointer = null,
            params object[] constructionParameters)
            where TState : State
        {
            NodeID nodeID = nodeIdFactory.GetNew();

            TState state = (TState)Activator.CreateInstance(typeof(TState), constructionParameters);
            state.OnTrigger += OnFlag;

            states.Add(nodeID, state);

            Node newNode = new Node(nodeID, parentId);

            nodes.Add(nodeID, newNode);
            enterParameters.Add(nodeID, enterParametersPointer);
            tickParameters.Add(nodeID, tickParametersPointer);
            exitParameters.Add(nodeID, exitParametersPointer);

            if (nodes.TryGetValue(parentId, out Node parent))
            {
                parent.Children.Add(nodeID);
            }

            return nodeID;
        }

        private NodeID GetDefaultLeaf(NodeID node)
        {
            Node current = nodes[node];

            while (current is { IsLeaf: false })
                current = nodes[current.DefaultChild];

            return current!.Id;
        }

        private Node GetDefaultLeaf(Node node)
        {
            return nodes.GetValueOrDefault(GetDefaultLeaf(node.Id));
        }

        public void ForceState(NodeID stateId)
        {
            if (!nodes.TryGetValue(stateId, out Node stateNode))
            {
                throw new Exception($"State of id {stateId} not registered");
            }

            NodeID nodeLeafId = GetDefaultLeaf(stateId);

            if (!nodes.ContainsKey(currentNodeID))
            {
                currentNodeID = nodeLeafId;
                EnterFromParent(nodeLeafId, NodeID.ROOT_ID);
                return;
            }

            ChangeState(nodeLeafId);
        }

        private void ChangeState(NodeID destinationState, Action onTransition = null)
        {
            if (!nodes.TryGetValue(destinationState, out Node destinationNode))
                throw new IndexOutOfRangeException(
                    $"BTNode ID: {destinationNode.Id}, is not contained in the node dictonary.");

            if (!destinationNode.IsLeaf)
                throw new Exception("Destination state should be a leaf but " + destinationState + " was provided");

            Node currentNode = nodes[currentNodeID];

            if (currentNode == destinationNode || currentNode.Id == destinationNode.Id)
                throw new Exception("Tried transitioning to self node. This is not implemented.");


            Node nodeLeafNode = GetDefaultLeaf(destinationNode);

            Node commonParent = FindCommonParent(currentNode, nodeLeafNode ?? destinationNode);

            ExitUntil(currentNode, commonParent);

            onTransition?.Invoke();

            currentNodeID = destinationState;

            EnterFromParent(destinationState, commonParent.Id);
        }

        private void ExitUntil(Node current, Node parent)
        {
            if (current == parent || current == null || parent == null)
                return;

            ExitNode(current);

            ExitUntil(nodes[current.Parent], parent);
        }

        private void EnterFromParent(NodeID destination, NodeID parent)
        {
            if (destination == parent || destination == null || parent == null)
                return;

            EnterFromParent(nodes[destination].Parent, parent);

            EnterNode(destination);
        }

        private void EnterNode(NodeID node)
        {
            if (node == NodeID.ROOT_ID)
                return;

            ExecuteBehaviour(GetOnEnterBehaviour(node));
        }

        public void Tick()
        {
            if (currentNodeID == NodeID.ROOT_ID || !states.ContainsKey(currentNodeID))
                return;

            TickFromRoot(currentNodeID, currentNodeID);
        }

        private void TickFromRoot(NodeID node, NodeID leaf)
        {
            if (node == NodeID.ROOT_ID)
                return;

            TickFromRoot(nodes[node].Parent, leaf);

            if (currentNodeID != leaf)
                return;

            ExecuteBehaviour(GetOnTickBehaviour(node));
        }

        private Node FindCommonParent(Node nodeA, Node nodeB)
        {
            HashSet<Node> parents = new HashSet<Node>();

            Node current = nodeA;

            while (current != null)
            {
                parents.Add(current);
                current = current.Parent != NodeID.ROOT_ID ? nodes[current.Parent] : null;
            }

            parents.Add(nodes[NodeID.ROOT_ID]);

            current = nodeB;

            while (current != null)
            {
                if (parents.Contains(current))
                    return current;

                if (current.Id == NodeID.ROOT_ID && current.Parent == NodeID.ROOT_ID)
                {
                    current = null;
                }
                else
                    current = current.Parent != NodeID.ROOT_ID ? nodes[current.Parent] : nodes[NodeID.ROOT_ID];
            }

            return current;
        }

        private void ExitNode(Node node)
        {
            if (node.Id == NodeID.ROOT_ID)
                return;

            ExecuteBehaviour(GetOnExitBehaviour(node.Id));
        }

        private BehaviourActions GetOnExitBehaviour(NodeID stateId)
        {
            return states[stateId].GetOnExitBehaviours(exitParameters[stateId]?.Invoke());
        }

        private void ExecuteBehaviour(BehaviourActions behaviourAction)
        {
            if (behaviourAction.Equals(default(BehaviourActions)))
                return;

            int executionOrder = 0;

            while (behaviourAction.MainThreadBehaviours is { Count: > 0 } ||
                   behaviourAction.MultiThreadableBehaviours is { Count: > 0 })
            {
                Task multiThreadableBehaviour = new Task(() =>
                {
                    if (behaviourAction.MultiThreadableBehaviours != null)
                    {
                        if (behaviourAction.MultiThreadableBehaviours.ContainsKey(executionOrder))
                        {
                            Parallel.ForEach(behaviourAction.MultiThreadableBehaviours[executionOrder], parallelOptions,
                                (behaviour) => { behaviour?.Invoke(); });
                            behaviourAction.MultiThreadableBehaviours.TryRemove(executionOrder, out _);
                        }
                    }
                });

                multiThreadableBehaviour.Start();

                if (behaviourAction.MainThreadBehaviours != null)
                {
                    if (behaviourAction.MainThreadBehaviours.ContainsKey(executionOrder))
                    {
                        foreach (Action mainThreadBehaviour in behaviourAction.MainThreadBehaviours[executionOrder])
                        {
                            mainThreadBehaviour?.Invoke();
                        }

                        behaviourAction.MainThreadBehaviours.Remove(executionOrder);
                    }
                }

                multiThreadableBehaviour?.Wait();
                executionOrder++;
            }

            behaviourAction.TransitionBehaviour?.Invoke();
        }

        public void RegisterTransition(NodeID originNodeID, string flag, NodeID destinationNodeID,
            Action onTransition = null)
        {
            if (!nodes.ContainsKey(originNodeID))
                throw new IndexOutOfRangeException($"NODE: {originNodeID} is NOT registred");

            if (!nodes.ContainsKey(destinationNodeID))
                throw new IndexOutOfRangeException($"NODE: {destinationNodeID} is NOT registred");

            if (!flagsAlias.TryGetValue(flag, out uint flagID))
                throw new IndexOutOfRangeException(
                    $"FLAG: {flag} is NOT registred. Before calling {nameof(RegisterTransition)} make sure to add {flag} by using {nameof(AddFlagAlias)}");

            if (!transitions.TryGetValue(originNodeID,
                    out Dictionary<uint, (NodeID destinationNodeID, Action onTransition)> dictionary))
            {
                dictionary = new Dictionary<uint, (NodeID, Action)>();
                transitions[originNodeID] = dictionary;
            }

            dictionary[flagID] = (destinationNodeID, onTransition);
        }

        public void Dispose()
        {
            foreach (Dictionary<uint, (NodeID destinationNodeID, Action onTransition)> transition in transitions.Values)
                foreach (uint key in transition.Keys)
                {
                    (NodeID destinationNodeID, Action onTransition) action = transition[key];
                    action.onTransition = null;
                    transition[key] = action;
                }
        }

        private sealed class NodeIdFactory
        {
            private uint lastAssignedID = 0;

            public NodeIdFactory()
            {
            }

            public NodeID GetNew()
            {
                return new NodeID(++lastAssignedID, GetHashCode());
            }
        }
    }
}