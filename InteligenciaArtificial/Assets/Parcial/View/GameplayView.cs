using System.IO;
using ianco99.ToolBox.Scheduling;
using ianco99.ToolBox.Services;
using Parcial.Architecture.GameLogic.Gameplay;
using Parcial.View.Console;
using Parcial.View.Mapping;
using Parcial.View.Resources;
using Parcial.View.Scene;
using UnityEngine;

namespace Parcial.View
{
    [ViewOf(typeof(Gameplay))]
    public sealed class GameplayView : MonoBehaviour
    {
        private TaskScheduler TaskScheduler => ServiceProvider.Instance.GetService<TaskScheduler>();
        private GameScene GameScene => ServiceProvider.Instance.GetService<GameScene>();

        private string BluprintsPath => Path.Combine(Application.streamingAssetsPath, "Blueprints", "Blueprints.xlsx");
        private string MapConfigPath => MapConfigIO.FilePath;


        [SerializeField] private Canvas gameCanvas;

        private Gameplay gameplay;
        private ConsoleView consoleView;

        private int mapX;
        private int mapY;

        private void Awake()
        {
            if (gameCanvas == null)
                throw new MissingComponentException("Missing canvas!");


            ViewArchitectureMap.Init();

            gameplay = new Gameplay(BluprintsPath, MapConfigPath);
            ServiceProvider.Instance.AddService<PrefabsRegistryView>(new PrefabsRegistryView());

            ServiceProvider.Instance.AddService<GameScene>
                (GameScene.AddSceneComponent<GameScene>("Scene", this.transform));

            consoleView = new ConsoleView();
        }

        private void Start()
        {
            gameplay.Init();
            GameScene.Init(gameCanvas);

            gameplay.LateInit();
            GameScene.LateInit();
        }

        void Update()
        {
            gameplay.Tick(Time.deltaTime);
            GameScene.Tick(Time.deltaTime);
        }

        private void OnDisable()
        {
            gameplay.Dispose();
            GameScene.Dispose();
            consoleView.Dispose();
        }
    }
}