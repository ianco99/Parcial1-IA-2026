using ianco99.ToolBox.Blueprints;
using ianco99.ToolBox.DataFlow;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Scheduling;
using ianco99.ToolBox.Services;
using Parcial.Architecture.Config;
using System;

namespace Parcial.Architecture.GameLogic.Gameplay
{
    public class Gameplay : IInitable, ITickable, IDisposable
    {
        private TaskScheduler TaskScheduler => ServiceProvider.Instance.GetService<TaskScheduler>();
        private MapConfigReader MapConfigReader => ServiceProvider.Instance.GetService<MapConfigReader>();
        private Scene Scene => ServiceProvider.Instance.GetService<Scene>();
        private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();


        public Gameplay(string blueprintsPath, string mapConfigPath)
        {
            ServiceProvider.Instance.AddService<EventBus>(new EventBus());
            ServiceProvider.Instance.AddService<BlueprintRegistry>(new BlueprintRegistry(blueprintsPath));
            ServiceProvider.Instance.AddService<BlueprintFieldsCache>(new BlueprintFieldsCache());
            ServiceProvider.Instance.AddService<BlueprintBinder>(new BlueprintBinder());
            ServiceProvider.Instance.AddService<TaskScheduler>(new TaskScheduler());
            ServiceProvider.Instance.AddService<MapConfigReader>(new MapConfigReader(mapConfigPath));
            ServiceProvider.Instance.AddService<Scene>(new Scene());
        }


        public void Init()
        {
            MapConfigReader.Init();
            Scene.Init();
        }

        public void LateInit()
        {
            MapConfigReader.LateInit();
            Scene.LateInit();
        }

        public void Tick(float deltaTime)
        {
            TaskScheduler.Tick(deltaTime);
            Scene.Tick(deltaTime);
            EventBus.Tick(deltaTime);
        }

        public void Dispose()
        {
            Scene.Dispose();
        }
    }
}