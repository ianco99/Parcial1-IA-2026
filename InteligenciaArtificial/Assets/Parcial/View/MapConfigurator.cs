using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Parcial.View
{
    public class MapConfigurator : MonoBehaviour
    {
        private const uint MinDimension = 1;
        private const uint MaxDimension = 256;
        private const uint MinAnthills = 2;

        [SerializeField] private TMP_InputField xInputField;
        [SerializeField] private TMP_InputField yInputField;
        [SerializeField] private TMP_InputField tileXInputField;
        [SerializeField] private TMP_InputField tileYInputField;
        [SerializeField] private TMP_InputField hormiguerosInputField;

        public void GenerateMap()
        {
            if (!uint.TryParse(xInputField.text, out uint xDimension) ||
                !uint.TryParse(yInputField.text, out uint yDimension))
            {
                Debug.LogWarning("Map dimensions must be whole positive numbers.");
                return;
            }

            if (xDimension < MinDimension || xDimension > MaxDimension ||
                yDimension < MinDimension || yDimension > MaxDimension)
            {
                Debug.LogWarning($"Map dimensions must be between {MinDimension} and {MaxDimension}.");
                return;
            }

            if (!uint.TryParse(hormiguerosInputField.text, out uint anthills))
            {
                Debug.LogWarning("Anthill amount must be a whole positive number.");
                return;
            }

            if (anthills < MinAnthills)
            {
                Debug.LogWarning($"There must be at least {MinAnthills} anthills.");
                return;
            }

            MapConfigIO.Save(new MapConfig(xDimension, yDimension, uint.Parse(tileXInputField.text), uint.Parse(tileYInputField.text), anthills));
            SceneManager.LoadScene("Game");
        }
    }

    public class MapConfig
    {
        public uint width;
        public uint height;
        public uint tileX;
        public uint tileY;
        public uint anthills;

        public MapConfig(uint width, uint height, uint tileX, uint tileY, uint anthills)
        {
            this.width = width;
            this.height = height;
            this.tileX = tileX;
            this.tileY = tileY;
            this.anthills = anthills;
        }
    }

    public static class MapConfigIO
    {
        public const string FileName = "map_config.bin";

        public static string FilePath => Path.Combine(Application.persistentDataPath, "Map", FileName);

        public static void Save(MapConfig config)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

            using (FileStream file = new FileStream(FilePath, FileMode.Create, FileAccess.Write))
            using (BinaryWriter writer = new BinaryWriter(file))
            {
                writer.Write(config.width);
                writer.Write(config.height);
                writer.Write(config.tileX);
                writer.Write(config.tileY);
                writer.Write(config.anthills);
            }
        }
    }
}