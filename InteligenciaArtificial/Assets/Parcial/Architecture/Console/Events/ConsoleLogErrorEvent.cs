using ianco99.ToolBox.Events;

namespace Parcial.Architecture.Console.Events
{
	public struct ConsoleLogErrorEvent : IEvent
	{
		public string Message;

		public void Assign(params object[] parameters)
		{
			Message = parameters[0] as string;
		}

		public void Reset()
		{
			Message = null;
		}
	}
}