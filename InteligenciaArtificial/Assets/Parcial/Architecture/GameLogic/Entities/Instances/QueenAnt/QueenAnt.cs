using System;
using System.Collections.Generic;
using ianco99.ToolBox.Services;
using KuznickiAgents.StatePattern;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.GameLogic.Entities.Instances.QueenAnt.States;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities
{
	public class QueenAnt : LivingEntity.LivingEntity
	{
		public const int QUEEN_ANT_HEALTH = 20;
		public const int QUEEN_ANT_SPEED = 1;
		public const int QUEEN_ANT_MOVE_TIME = 1;
		public const float EGG_COOLDOWN = 30f;
		public const int EGG_FOOD_COST = 20;
		public const int BUILDERS_PER_WARRIOR = 2;

		private EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();

		private Fsm fsm;

		private Func<float> deltaTimeAction;
		private float cacheDeltaTime;

		public Point eggSpawnPoint;
		public EggAnt.EggAnt.HatchTypes eggHatchType;

		internal enum States
		{
			StandBy,
			SpawnEgg
		}

		internal enum Flags
		{
			OnEggToSpawn,
			OnBackToIdle
		}
		
		protected QueenAnt(uint ID, Coordinate coordinate, int teamID) : base(ID, coordinate, teamID, QUEEN_ANT_HEALTH, QUEEN_ANT_SPEED, QUEEN_ANT_MOVE_TIME)
		{
		}

        public override void Init()
        {
            base.Init();

            deltaTimeAction = GetDeltaTime;

            Dictionary<string, int> flagAliases = new Dictionary<string, int>()
            {
	            {Flags.OnEggToSpawn.ToString(), (int)Flags.OnEggToSpawn},
	            {Flags.OnBackToIdle.ToString(), (int)Flags.OnBackToIdle},
            };
			
            fsm = new Fsm(2, flagAliases);
            
            fsm.AddState<StandByState>((int)States.StandBy, constructionParameters: new object[] {teamID, this}, updateParametersPointer: () => new object[]{deltaTimeAction});
            fsm.AddState<SpawnEggState>((int)States.SpawnEgg, constructionParameters: new object[] {this});
            
            fsm.RegisterTransition((int)States.StandBy, Flags.OnEggToSpawn.ToString(), (int)States.SpawnEgg);
            fsm.RegisterTransition((int)States.SpawnEgg, Flags.OnBackToIdle.ToString(),(int)States.StandBy);
            
            fsm.ForceState((int)States.StandBy);
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            cacheDeltaTime = deltaTime;
            fsm.Update();
        }

        public EggAnt.EggAnt.HatchTypes DecideEggType()
        {
	        int builders = 0;
	        int warriors = 0;

	        foreach (BuilderAnt builderAnt in EntityRegistry.BuilderAnts)
	        {
		        if (builderAnt.teamID == teamID)
			        builders++;
	        }

	        foreach (Instances.WarriorAnt.WarriorAnt warriorAnt in EntityRegistry.WarriorAnts)
	        {
		        if (warriorAnt.teamID == teamID)
			        warriors++;
	        }

	        foreach (EggAnt.EggAnt eggAnt in EntityRegistry.FilterEntities<EggAnt.EggAnt>())
	        {
		        if (eggAnt.teamID != teamID)
			        continue;

		        if (eggAnt.HatchType == EggAnt.EggAnt.HatchTypes.WarriorAnt)
			        warriors++;
		        else
			        builders++;
	        }

	        return warriors * BUILDERS_PER_WARRIOR < builders
		        ? EggAnt.EggAnt.HatchTypes.WarriorAnt
		        : EggAnt.EggAnt.HatchTypes.BuilderAnt;
        }

        private float GetDeltaTime()
        {
	        return cacheDeltaTime;
        }
        
        private void Dispose()
        {
	        
        }
    }
}