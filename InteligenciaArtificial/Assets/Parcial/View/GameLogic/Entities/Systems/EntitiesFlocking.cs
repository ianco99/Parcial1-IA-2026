using System;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using KuznickiAgents.Flocking;
using Parcial.Architecture.GameLogic.Entities.Events;
using Parcial.Architecture.GameLogic.Entities.LivingEntity;
using Parcial.View.GameLogic.Entities.Instances.LivingEntity.Events;
using UnityEngine;
using ZooArchitect.Architecture.Entities.Events;

namespace Parcial.View.GameLogic.Entities.Systems
{
	public class EntitiesFlocking : IService, IDisposable
	{
		public bool IsPersistance => false;
		
		EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
		EntityRegistryView EntityRegistryView => ServiceProvider.Instance.GetService<EntityRegistryView>();
		

		private FlockingAlgorithm flocking;

		public EntitiesFlocking()
		{
			flocking = new FlockingAlgorithm();
			EventBus.SubscribeOnlyMainThread<FlockingAgentRegisteredEvent>(OnEntityCreated);
		}

		private void OnEntityCreated(in FlockingAgentRegisteredEvent callback)
		{
			if (!EntityRegistryView.Contains(callback.id))
				return;

			flocking.AddAgent((IFlockingAgent)EntityRegistryView[callback.id]);
		}

		internal void RemoveAgent(IFlockingAgent flockingAgent)
		{
			flocking.RemoveAgent(flockingAgent);
		}
		
		public Vector3 GetAlignment(IFlockingAgent flockingAgent)
		{
			return CastV3ToUnity(flocking.Alignment(flockingAgent));
		}
		
		public Vector3 GetCohesion(IFlockingAgent flockingAgent)
		{
			return CastV3ToUnity(flocking.Cohesion(flockingAgent));
		}
		
		public Vector3 GetSeparation(IFlockingAgent flockingAgent)
		{
			return CastV3ToUnity(flocking.Separation(flockingAgent));
		}
		
		public Vector3 GetDirection(IFlockingAgent flockingAgent, Vector3 target)
		{
			return CastV3ToUnity(flocking.Direction(flockingAgent, CastV3ToNumerics(target)));
		}

		private Vector3 CastV3ToUnity(System.Numerics.Vector3 vector)
		{
			if (float.IsNaN(vector.X) || float.IsNaN(vector.Y) || float.IsNaN(vector.Z))
				return Vector3.zero;

			return new Vector3(vector.X, vector.Y, vector.Z);
		}

		private System.Numerics.Vector3 CastV3ToNumerics(Vector3 vector)
		{
			return new System.Numerics.Vector3(vector.x, vector.y, vector.z);
		}

		public void Dispose()
		{
			EventBus.UnsubscribeOnlyMainThread<FlockingAgentRegisteredEvent>(OnEntityCreated);
		}
	}
}