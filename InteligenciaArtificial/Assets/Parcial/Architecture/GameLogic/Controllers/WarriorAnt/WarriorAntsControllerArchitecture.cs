using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture.GameLogic.Entities.Events;
using Parcial.Architecture.GameLogic.Entities.Food;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.GameLogic.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ianco99.ToolBox.DataFlow;

namespace Parcial.Architecture.GameLogic.Controllers.WarriorAnt
{
	public class WarriorAntsControllerArchitecture : IDisposable, ITickable
	{
		private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
		private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

		private Scene Scene => ServiceProvider.Instance.GetService<Scene>();

		List<Entities.Instances.WarriorAnt.WarriorAnt> warriorAnts = new List<Entities.Instances.WarriorAnt.WarriorAnt>();
		Dictionary<int, int> storedFoodPerTeam = new Dictionary<int, int>();
		
		private readonly ConcurrentQueue<(uint attacker, uint target, bool attackerWasOnCooldown)> pendingAttacks =
			new ConcurrentQueue<(uint attacker, uint target, bool attackerWasOnCooldown)>();

		private readonly Random random = new Random();

		public WarriorAntsControllerArchitecture()
		{
			EventBus.Subscribe<RequestPickUpFoodEvent>(OnRequestPickUpFood);
			EventBus.Subscribe<AttackExecutedEvent>(OnAttackExecuted);
		}

		private void OnAttackExecuted(in AttackExecutedEvent callback)
		{
			pendingAttacks.Enqueue((callback.attackerID, callback.targetID, callback.attackerWasOnCooldown));
		}

		private void ResolveAttacks()
		{
			while (pendingAttacks.TryDequeue(out (uint attacker, uint target, bool attackerWasOnCooldown) attack))
			{
				if (!EntityRegistry.Contains(attack.attacker) || !EntityRegistry.Contains(attack.target))
				{
					Entities.Instances.WarriorAnt.WarriorAnt.Log(attack.attacker, $"Attack on {attack.target} dropped: attacker or target already dead");
					continue;
				}

				Entity attacker = EntityRegistry[attack.attacker];
				Entity target = EntityRegistry[attack.target];

				switch (target)
				{
					case Entities.Instances.WarriorAnt.WarriorAnt targetWarrior:
						bool targetOnCooldown = targetWarrior.IsOnAttackCooldown;
						bool attackerDies;
						string reason;

						if (attack.attackerWasOnCooldown != targetOnCooldown)
						{
							attackerDies = attack.attackerWasOnCooldown;
							reason = "the loser was on attack cooldown";
						}
						else
						{
							attackerDies = random.Next(2) == 0;
							reason = "50/50";
						}

						Kill(attack.attacker, attackerDies ? attacker : target, $"duel between warriors {attack.attacker} and {attack.target} ({reason})");
						break;

					case Entities.BuilderAnt _:
					case Entities.QueenAnt _:
					case Entities.EggAnt.EggAnt _:
						Kill(attack.attacker, target, $"attacked by warrior {attack.attacker}");
						break;
				}
			}
		}

		private void Kill(uint attackerID, Entity victim, string reason)
		{
			Entities.Instances.WarriorAnt.WarriorAnt.Log(attackerID, $"Attack resolved: {victim.GetType().Name} {victim.ID} (team {victim.teamID}) dies, {reason}");

			EntityRegistry.UnRegister(victim);

			if (victim is Entities.QueenAnt)
				EventBus.Raise<QueenKilledEvent>(victim.ID, victim.teamID);
		}

		private readonly object pickUpLock = new object();

		private void OnRequestPickUpFood(in RequestPickUpFoodEvent callback)
		{
			lock (pickUpLock)
			{
				PickUpFood(callback.collector, callback.collectorTeamId, callback.amount);
			}
		}

		private void PickUpFood(uint collector, int collectorTeamId, int amount)
		{
			Entities.Instances.WarriorAnt.WarriorAnt ant =
				(Entities.Instances.WarriorAnt.WarriorAnt)EntityRegistry[collector];

			if (!Scene.IsTeamAnthillTile(ant.Coordinate.Origin, collectorTeamId))
				return;

			List<Food> foods = new List<Food>(EntityRegistry.Foods);

			int pickedUp = 0;

			foreach (Food food in foods)
			{
				if (!food.IsStored || food.teamID != collectorTeamId)
					continue;

				while (pickedUp < amount && food.CurrentAmount > 0)
				{
					food.GetCollected();
					pickedUp++;
				}

				if (pickedUp >= amount)
					break;
			}

			if (pickedUp > 0)
				ant.StoreFood(pickedUp);
		}

		public void Dispose()
		{
			EventBus.UnSubscribe<RequestPickUpFoodEvent>(OnRequestPickUpFood);
			EventBus.UnSubscribe<AttackExecutedEvent>(OnAttackExecuted);
		}

		public void Tick(float deltaTime)
		{
			warriorAnts.Clear();
			warriorAnts.AddRange(EntityRegistry.WarriorAnts);

			storedFoodPerTeam.Clear();

			foreach (Entities.Instances.WarriorAnt.WarriorAnt warriorAnt in warriorAnts)
			{
				if (!storedFoodPerTeam.ContainsKey(warriorAnt.teamID))
					storedFoodPerTeam.Add(warriorAnt.teamID, Scene.GetStoredFoodAmount(warriorAnt.teamID));

				warriorAnt.SetTeamFood(storedFoodPerTeam[warriorAnt.teamID]);
			}
			
			Parallel.ForEach(warriorAnts, (warriorAnt) =>
			{
				warriorAnt.Tick(deltaTime);
			});

			ResolveAttacks();
		}
	}
}