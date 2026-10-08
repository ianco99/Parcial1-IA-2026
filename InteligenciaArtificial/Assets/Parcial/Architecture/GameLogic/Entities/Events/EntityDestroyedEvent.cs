using ianco99.ToolBox.Events;

namespace ZooArchitect.Architecture.Entities.Events
{
    public struct EntityDestroyedEvent : IEvent
    {
        public uint entityID;
        public void Assign(params object[] parameters)
        {
            entityID = (uint)parameters[0];
        }

        public void Reset()
        {
            entityID = default(uint);
        }
    }
}