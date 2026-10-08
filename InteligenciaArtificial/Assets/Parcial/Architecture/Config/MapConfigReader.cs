using System.IO;
using ianco99.ToolBox.DataFlow;
using ianco99.ToolBox.Services;
using Parcial.Architecture.Console;
using Parcial.Architecture.Exceptions;

namespace Parcial.Architecture.Config
{
    public sealed class MapConfigReader : IService, IInitable
    {
        public bool IsPersistance => false;

        private const int FIELD_AMOUNT = 5;
        private const int EXPECTED_BYTES = FIELD_AMOUNT * sizeof(uint);
        private const uint MINIMUM_ANTHILLS = 2;

        private string mapConfigPath;

        private uint mapWidth;
        public uint MapWidth => mapWidth;

        private uint mapHeight;
        public uint MapHeight => mapHeight;

        private uint tileWidth;
        public uint TileWidth => tileWidth;

        private uint tileHeight;
        public uint TileHeight => tileHeight;

        private uint anthillAmount;
        public uint AnthillAmount => anthillAmount;

        public MapConfigReader(string mapConfigPath)
        {
            this.mapConfigPath = mapConfigPath;
        }

        public void Init()
        {
            if (!File.Exists(mapConfigPath))
                throw new DataEntryException($"Missing map configuration at {mapConfigPath}. Run the GameConfig scene before the Game scene.");

            using (FileStream file = new FileStream(mapConfigPath, FileMode.Open, FileAccess.Read))
            using (BinaryReader reader = new BinaryReader(file))
            {
                if (file.Length < EXPECTED_BYTES)
                    throw new DataEntryException($"{mapConfigPath} is {file.Length} bytes, expected {EXPECTED_BYTES}. Re-run the GameConfig scene to rewrite it.");

                mapWidth = reader.ReadUInt32();
                mapHeight = reader.ReadUInt32();
                tileWidth = reader.ReadUInt32();
                tileHeight = reader.ReadUInt32();
                anthillAmount = reader.ReadUInt32();
            }

            if (anthillAmount < MINIMUM_ANTHILLS)
                throw new BrokenGameRuleException($"Map configuration requests {anthillAmount} anthills, the minimum is {MINIMUM_ANTHILLS}.");

            GameConsole.Log($"Map config: {mapWidth}x{mapHeight} map, {tileWidth}x{tileHeight} tiles, {anthillAmount} anthills.");
        }

        public void LateInit()
        {
        }
    }
}
