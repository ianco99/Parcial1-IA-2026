using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture.Console.Events;

namespace Parcial.Architecture.Console
{
	public static class GameConsole 
	{
		private static EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

		public static void Log(string message) 
		{
			EventBus.Raise<ConsoleLogEvent>(message);
		}

		public static void Warning(string message)
		{
			EventBus.Raise<ConsoleWarningEvent>(message);
		}

		public static void Error(string message)
		{
			EventBus.Raise<ConsoleErrorEvent>(message);
		}
	}
}