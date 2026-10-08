using System;
using System.Numerics;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using KuznickiAgents.Flocking;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;
using Parcial.View.GameLogic.Entities.Systems;
using Parcial.View.Mapping;

namespace Parcial.View.GameLogic.Entities.Instances.LivingEntity
{
	[ViewOf(typeof(Architecture.GameLogic.Entities.LivingEntity.LivingEntity))]
	internal class LivingEntityView : EntityView, IFlockingAgent
	{
		public override Type ArchitectureEntityType => typeof(Architecture.GameLogic.Entities.LivingEntity.LivingEntity);
		EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
		EntitiesFlocking EntitiesFlocking => ServiceProvider.Instance.GetService<EntitiesFlocking>();
		EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

		public float alignmentFactor = 0.5f;
		public float cohesionFactor = 0.25f;
		public float separationFactor = 1.0f;
		public float directionFactor = 2.0f;
		public float arriveGain = 4.0f;
		public float turnSpeed = 10.0f;

		private UnityEngine.Vector3 target;

		public override void LateInit()
		{
			base.LateInit();
			target = transform.position;
		}

		public override void Move(Coordinate coordinate)
		{
			target = GameScene.CoordinateToWorld(coordinate);
		}

		public override void Tick(float deltaTime)
		{
			base.Tick(deltaTime);
			
			float distance = UnityEngine.Vector3.Distance(transform.position, target);
			float step = UnityEngine.Mathf.Min(distance * arriveGain * deltaTime, distance);
			transform.position += transform.up * step;

			UnityEngine.Vector3 steering = ACS();
			if (steering != UnityEngine.Vector3.zero)
			{
				float angle = UnityEngine.Vector3.SignedAngle(transform.up, steering, UnityEngine.Vector3.forward);
				transform.Rotate(0.0f, 0.0f, angle * UnityEngine.Mathf.Min(turnSpeed * deltaTime, 1.0f));
			}
		}

		public UnityEngine.Vector3 ACS()
		{
			UnityEngine.Vector3 ACS = (EntitiesFlocking.GetAlignment(this) * alignmentFactor) +
			                         (EntitiesFlocking.GetCohesion(this) * cohesionFactor) +
			                         (EntitiesFlocking.GetSeparation((this)) * separationFactor) +
			                         (EntitiesFlocking.GetDirection(this, target) * directionFactor);

			return ACS.normalized;
		}
		
		public Vector3 GetPosition()
		{
			return new Vector3(transform.position.x, transform.position.y, transform.position.z);
		}

		public float GetDetectionRadious()
		{
			return 2.0f;
		}
	}
}