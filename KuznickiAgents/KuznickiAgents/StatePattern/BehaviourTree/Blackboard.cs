using System.Collections.Generic;
using System.Threading.Tasks;

namespace KuznickiAgents.StatePattern.BehaviourTree
{
	public sealed class Blackboard 
	{
		public ParallelOptions ParallelOptions { get; private set; }

		private readonly Dictionary<string, object> dataElements;
		private readonly Dictionary<string, IBlackboardReference> referenceElements;

		public Blackboard()
		{
			dataElements = new Dictionary<string, object>();
			referenceElements = new Dictionary<string, IBlackboardReference>();
			ParallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = 32 };
		}

		public bool AddData(string key, object element) 
		{
			return dataElements.TryAdd(key, element); 
		}

		public bool TryGetData(string key, out object element) 
		{
			element = null;
			return dataElements.TryGetValue(key, out element);
		}

		public bool TryGetDataAs<T>(string key, out T element) where T : unmanaged
		{
			element = default;
			object temp = null;
			bool result = dataElements.TryGetValue(key, out temp);
			element = (T)temp;
			return result;
		}

		public bool AddReference(string key, IBlackboardReference reference)
		{
			return referenceElements.TryAdd(key, reference);
		}

		public bool TryGetReference(string key, out IBlackboardReference reference)
		{
			reference = null;
			return referenceElements.TryGetValue(key, out reference);
		}

		public bool TryGetReferenceAs<T>(string key, out T reference) where T : IBlackboardReference
		{
			reference = default;
			IBlackboardReference temp = null;
			bool result = referenceElements.TryGetValue(key, out temp);
			reference = (T)temp;
			return result;
		}

		public bool SetData(string key, object element)
		{
			if(dataElements.ContainsKey(key))
			{
				dataElements[key] = element;
				return true;
			}

			return false;
		}
	}
}

