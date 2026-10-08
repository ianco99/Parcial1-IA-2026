using ianco99.ToolBox.Events;

namespace Parcial.Architecture.GameLogic.Map.Events
{
    public struct GridCreatedEvent : IEvent
    {
        public uint sizeX;
        public uint sizeY;
        public uint tileSizeX;
        public uint tileSizeY;
        
        public void Assign(params object[] parameters)
        {
            sizeX = (uint)parameters[0];
            sizeY = (uint)parameters[1];
            tileSizeX = (uint)parameters[2];
            tileSizeY = (uint)parameters[3];
        }

        public void Reset()
        {
            sizeX = 0;
            sizeY = 0;
            tileSizeX = 0;
            tileSizeY = 0;
        }
    }
}