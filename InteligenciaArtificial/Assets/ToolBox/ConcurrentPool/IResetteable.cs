namespace ianco99.ToolBox.Pool
{
    public interface IResetteable
    {
        void Assign(params object[] parameters);
        void Reset();
    }
}