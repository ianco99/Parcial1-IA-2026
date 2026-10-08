namespace KuznickiAgents.StatePattern
{
	public readonly struct NodeID
	{
		//It can also be interpreted as a NULL, depending of use.
		public static readonly NodeID ROOT_ID = default;

		private readonly uint ID;
		private readonly int originFactoryHash;

		public NodeID(uint ID, int originFactoryHash)
		{
			this.ID = ID;
			this.originFactoryHash = originFactoryHash;
		}

		public override bool Equals(object obj)
		{
			return obj is NodeID iD &&
			       ID == iD.ID &&
			       originFactoryHash == iD.originFactoryHash;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static bool operator ==(NodeID a, NodeID b)
		{
			return a.ID == b.ID && a.originFactoryHash == b.originFactoryHash;
		}

		public static bool operator !=(NodeID a, NodeID b)
		{
			return !(a == b);
		}
	}
}