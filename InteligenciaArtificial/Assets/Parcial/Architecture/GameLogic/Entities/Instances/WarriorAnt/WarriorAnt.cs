using System;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using KuznickiAgents.StatePattern;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Entities;
using Parcial.Architecture.GameLogic.Entities.Events;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.GameLogic.Entities.LivingEntity;
using Parcial.Architecture.GameLogic.Entities.WarriorAnt.States;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.Instances.WarriorAnt
{
	public class WarriorAnt : LivingEntity.LivingEntity
	{
		//Creeper
		public enum States
		{
			MoveToAnthill,
			ChangeTarget,
			MoveToTarget,
			AttackTarget,
		}

		public enum Flags
		{
			OnFoundTarget,
			OnTargetLost,
			OnBackToAnthill,
			OnAnthillReached
		}

		public enum Targets
		{
			EggAnt = 1,
			BuilderAnt = 5,
			WarriorAnt = 10,
			QueenAnt = 50
		}

		public const int WARRIOR_ANT_HEALTH = 20;
		public const int WARRIOR_ANT_SPEED = 1;
		public const float WARRIOR_ANT_MOVE_TIME = 0.1f;
		public const float ATTACK_COOLDOWN = 10f;

		public static bool LogsEnabled = false;

		EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
		EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

		private Hsm hsm;

		private uint currentTargetID;

		private Point homePoint;
		private float cachedDeltaTime;
		
		private int storedFood;

		private int teamFood;

		public int StoredFood => storedFood;

		private float attackCooldownLeft;

		public bool IsOnAttackCooldown => attackCooldownLeft > 0;

		protected WarriorAnt(uint ID, Coordinate coordinate, int teamID) : base(ID, coordinate, teamID,
			WARRIOR_ANT_HEALTH, WARRIOR_ANT_SPEED, WARRIOR_ANT_MOVE_TIME)
		{
		}

		public override void Init()
		{
			base.Init();

			homePoint = Coordinate.Origin;

			hsm = new Hsm();
			
			hsm.AddFlagAlias(Flags.OnFoundTarget.ToString());
			hsm.AddFlagAlias(Flags.OnTargetLost.ToString());
			hsm.AddFlagAlias(Flags.OnBackToAnthill.ToString());
			hsm.AddFlagAlias(Flags.OnAnthillReached.ToString());

			NodeID moveToAnthillStateID = hsm.AddState<MoveState>((int)States.MoveToAnthill,
				enterParametersPointer: () => new object[] { (Point?)homePoint, ID }, tickParametersPointer: () => new object[]{(Point?)homePoint, ID},
				constructionParameters: new object[]{0, Flags.OnAnthillReached.ToString()});

			NodeID chooseAttackTargetStateID = hsm.AddState<ChangeTargetState>((int)States.ChangeTarget,
				tickParametersPointer: () => new object[] { cachedDeltaTime, teamFood, teamID, ID},
				constructionParameters: new object[]{(Action<uint>)ChangeTarget, (Action<int>)RequestPickUpFood, (Func<int>)(() => storedFood) });
			
			NodeID moveToTargetStateID = hsm.AddState<MoveState>((int)States.MoveToTarget,
				enterParametersPointer: () => new object[] { GetTargetPoint(), ID }, tickParametersPointer: () => new object[]{GetTargetPoint(), ID},
				constructionParameters: new object[]{1, string.Empty});

			NodeID attackTargetStateID = hsm.AddState<AttackState>((int)States.AttackTarget,
				parentId: moveToTargetStateID, tickParametersPointer: () => new object[]{currentTargetID, ID},
				constructionParameters: new object[]{(Func<uint, bool>)Attack}
			);
			
			hsm.RegisterTransition(chooseAttackTargetStateID, Flags.OnFoundTarget.ToString(), attackTargetStateID);
			hsm.RegisterTransition(attackTargetStateID, Flags.OnBackToAnthill.ToString(), moveToAnthillStateID);
			hsm.RegisterTransition(attackTargetStateID, Flags.OnTargetLost.ToString(), moveToAnthillStateID);
			hsm.RegisterTransition(moveToAnthillStateID, Flags.OnAnthillReached.ToString(), chooseAttackTargetStateID);

			hsm.ForceState(chooseAttackTargetStateID);

			Log(ID, $"Init: team {teamID} at ({Coordinate.Origin.x}, {Coordinate.Origin.y}), HSM built and forced to {States.ChangeTarget}");
		}

		public override void Tick(float deltaTime)
		{
			base.Tick(deltaTime);
			cachedDeltaTime = deltaTime;

			if (attackCooldownLeft > 0)
				attackCooldownLeft -= deltaTime;

			//Log(ID, $"Tick: dt={deltaTime}, pos=({Coordinate.Origin.x}, {Coordinate.Origin.y}), storedFood={storedFood}, currentTargetID={currentTargetID}, hasPath={HasPath}");

			hsm.Tick();
		}

		private Point? GetTargetPoint()
		{
			if (!EntityRegistry.Contains(currentTargetID))
				return null;

			return EntityRegistry[currentTargetID].Coordinate.Origin;
		}

		private void ChangeTarget(uint targetID)
		{
			Log(ID, $"ChangeTarget: {currentTargetID} -> {targetID}");

			currentTargetID = targetID;
		}

		public void RequestPickUpFood(int amount)
		{
			Log(ID, $"RequestPickUpFood: asking team {teamID} anthill for {amount} food (storedFood={storedFood})");

			EventBus.Raise<RequestPickUpFoodEvent>(ID, teamID, amount);
		}

		public void SetTeamFood(int amount)
		{
			teamFood = amount;
		}

		public void StoreFood(int amount)
		{
			storedFood += amount;

			Log(ID, $"StoreFood: +{amount} -> storedFood={storedFood}");
		}

		public bool Attack(uint targetID)
		{
			if (!EntityRegistry.Contains(targetID))
			{
				Log(ID, $"Attack: target {targetID} is not registered -> no attack");
				return false;
			}

			int cost = GetAttackCost(EntityRegistry[targetID]);

			if (cost <= 0 || storedFood < cost)
			{
				Log(ID, $"Attack: target {targetID} ({EntityRegistry[targetID].GetType().Name}) costs {cost}, storedFood={storedFood} -> no attack");
				return false;
			}

			storedFood -= cost;

			Log(ID, $"Attack: hit target {targetID} ({EntityRegistry[targetID].GetType().Name}) for {cost} food -> storedFood={storedFood}");

			bool wasOnCooldown = IsOnAttackCooldown;
			attackCooldownLeft = ATTACK_COOLDOWN;

			EventBus.Raise<AttackExecutedEvent>(ID, targetID, wasOnCooldown);

			return true;
		}

		public static void Log(uint antID, string message)
		{
			if (LogsEnabled)
				GameConsole.Log($"[WarriorAnt HSM] ant {antID} {message}");
		}

		public static int GetAttackCost(Entity target)
		{
			switch (target)
			{
				case EggAnt.EggAnt _: return (int)Targets.EggAnt;
				case BuilderAnt _: return (int)Targets.BuilderAnt;
				case WarriorAnt _: return (int)Targets.WarriorAnt;
				case Entities.QueenAnt _: return (int)Targets.QueenAnt;
				default: return 0;
			}
		}
	}
}