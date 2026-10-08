using ianco99.ToolBox.DataFlow;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Controllers.Anthill;
using Parcial.Architecture.GameLogic.Controllers.Anthill.Events;
using Parcial.Architecture.GameLogic.Controllers.SpawnEntities.Events;
using Parcial.Architecture.GameLogic.Entities;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;
using System;
using System.Threading.Tasks;

namespace Parcial.Architecture.GameLogic.Controllers.SpawnEntities
{
    public class SpawnBuilderAntControllerArchitecture : IInitable, IDisposable
    {
		private int teamID;

		EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
        Scene Scene => ServiceProvider.Instance.GetService<Scene>();
        EntitiesLogic EntitiesLogic => ServiceProvider.Instance.GetService<EntitiesLogic>();

        public SpawnBuilderAntControllerArchitecture(int teamID)
        {
            this.teamID = teamID;

            EventBus.Subscribe<SpawnRequestEvent<Entities.BuilderAnt>>(OnBuilderAntSpawned);
        }

        private void OnBuilderAntSpawned(in SpawnRequestEvent<Entities.BuilderAnt> callback)
        {
            if(callback.teamId != teamID) return;


            EventBus.Raise<SpawnRequestAcceptedEvent<Entities.BuilderAnt>>(callback.blueprintToSpawn, callback.coordinateToSpawn,
                    TableNames.ANTS_TABLE_NAME, callback.teamId);
        }

        public void Dispose()
        {
            EventBus.UnSubscribe<SpawnRequestEvent<Entities.BuilderAnt>>(OnBuilderAntSpawned);
        }

        public void Init()
        {

        }

        public void LateInit()
        {
            Coordinate anthill = Scene.GetSpawnTeamControllerArchitecture(teamID)
                .SpawnAnthillControllerArchitecture.MyAnthill;


            Point[] points = Scene.GetEmptyAnthillPoints(anthill, teamID);

            for (int i = 0; i < 5; i++)
            {
                EventBus.Raise<SpawnRequestEvent<Entities.BuilderAnt>>(EntitiesLogic.GetBuilderAntBlueprint(), new Coordinate(points[i]), teamID);
            }
        }
    }
}