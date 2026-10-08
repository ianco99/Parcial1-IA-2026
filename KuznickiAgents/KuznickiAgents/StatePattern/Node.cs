using System.Collections.Generic;

namespace KuznickiAgents.StatePattern
{
	public class Node
	{
		private readonly List<NodeID> children;
		public NodeID DefaultChild => children.Count > 0 ? children[0] : default(NodeID);
		public List<NodeID> Children => children;

		public NodeID Parent { get; private set; }
		public NodeID Id { get; private set; }
		public bool IsLeaf => Children.Count == 0;

		public Node(NodeID id, NodeID parent)
		{
			children = new List<NodeID>();

			Id = id;
			Parent = parent;
		}

		public void AddChild(NodeID id)
		{
			children.Add(id);
		}
	}
}