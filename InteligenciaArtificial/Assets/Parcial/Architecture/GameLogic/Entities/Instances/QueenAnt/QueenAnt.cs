using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using KuznickiAgents.StatePattern;
using Parcial.Architecture.GameLogic.Controllers.SpawnEntities.Events;
using Parcial.Architecture.GameLogic.Entities.Events;
using Parcial.Architecture.GameLogic.Entities.Instances.QueenAnt.States;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;
using System;
using System.Collections.Generic;

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
        private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        private Scene Scene => ServiceProvider.Instance.GetService<Scene>();

        private Fsm fsm;

        private Func<float> deltaTimeAction;
        private float cacheDeltaTime;

        public Point eggSpawnPoint;
        public EggAnt.EggAnt.HatchTypes eggHatchType;


        private bool isAwaitingResponse;
        private bool isRequestAccepted;
        private bool isRequestDeclined;

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

            fsm.AddState<StandByState>((int)States.StandBy, updateParametersPointer: () => new object[] { GetDeltaTime(), teamID });
            fsm.AddState<SpawnEggState>((int)States.SpawnEgg, enterParametersPointer: () => new object[] { (Action<bool>)SetAwaitingResponse, (Action<bool>)SetRequestAccepted, (Action<bool>)SetRequestDeclined },
                updateParametersPointer: () => new object[]
                {
                    eggSpawnPoint,
                    teamID,
                    (Action)DecideEggType,
                    (Action<bool>)SetAwaitingResponse,
                    (Action<bool>)SetRequestDeclined,
                    (Func<bool>)TryEggPoint,
                    isAwaitingResponse,
                    isRequestAccepted,
                    isRequestDeclined
                });

            fsm.RegisterTransition((int)States.StandBy, Flags.OnEggToSpawn.ToString(), (int)States.SpawnEgg);
            fsm.RegisterTransition((int)States.SpawnEgg, Flags.OnBackToIdle.ToString(), (int)States.StandBy);

            fsm.ForceState((int)States.StandBy);

            EventBus.Subscribe<EntityCreatedEvent<EggAnt.EggAnt>>(OnEggCreated);
            EventBus.Subscribe<SpawnRequestRejectedEvent<EggAnt.EggAnt>>(OnSpawnRequestDeclined);
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            cacheDeltaTime = deltaTime;
            fsm.Update();
        }

        public bool TryEggPoint()
        {
            Coordinate anthill = Scene.GetSpawnTeamControllerArchitecture(teamID)
                .SpawnAnthillControllerArchitecture.MyAnthill;

            Point point = Scene.GetEmptyAnthillPoint(anthill, teamID);
            if (point == default)
                return false;

            eggSpawnPoint = point;
            return true;
        }

        private void DecideEggType()
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

            eggHatchType = warriors * BUILDERS_PER_WARRIOR < builders
                ? EggAnt.EggAnt.HatchTypes.WarriorAnt
                : EggAnt.EggAnt.HatchTypes.BuilderAnt;
        }

        private float GetDeltaTime()
        {
            return cacheDeltaTime;
        }

        private void Dispose()
        {
            EventBus.UnSubscribe<EntityCreatedEvent<EggAnt.EggAnt>>(OnEggCreated);
            EventBus.UnSubscribe<SpawnRequestRejectedEvent<EggAnt.EggAnt>>(OnSpawnRequestDeclined);
        }

        private void OnEggCreated(in EntityCreatedEvent<EggAnt.EggAnt> callback)
        {
            if (callback.entityTeamID != teamID || !isAwaitingResponse)
                return;

            EntityRegistry.GetAs<EggAnt.EggAnt>(callback.EntityCreatedId).HatchType = eggHatchType;

            isAwaitingResponse = false;
            isRequestAccepted = true;
        }

        private void OnSpawnRequestDeclined(in SpawnRequestRejectedEvent<EggAnt.EggAnt> callback)
        {
            if (callback.teamId != teamID || !isAwaitingResponse)
                return;

            isAwaitingResponse = false;
            isRequestDeclined = true;
        }

        public void SetAwaitingResponse(bool value)
        {
            isAwaitingResponse = value;
        }

        public void SetRequestDeclined(bool value)
        {
            isRequestDeclined = value;
        }

        public void SetRequestAccepted(bool value)
        { isRequestAccepted = value; }
    }
}