using System.Collections.Generic;
using Parcial.Architecture;

namespace Parcial.View
{
	internal static class TableNamesView
	{
		internal static readonly string[] PREFAB_TABLES = new string[]
		{
			//ANIMALS_VIEW_TABLE_NAME,
			ANTS_VIEW_TABLE_NAME,
			//INFRASTRUCTURE_VIEW_TABLE_NAME,
			//WORKERS_VIEW_TABLE_NAME,
			//VISITORS_VIEW_TABLE_NAME,
			TILES_VIEW_TABLE_NAME,
			UI_VIEW_TABLE_NAME,
			CAMERA_VIEW_TABLE_NAME,
			//FEEDBACK_VIEW_TABLE_NAME
		};

		internal const string ANIMALS_VIEW_TABLE_NAME = "Animals View";
		internal const string ANTS_VIEW_TABLE_NAME = "Ants View";
		internal const string INFRASTRUCTURE_VIEW_TABLE_NAME = "Infrastructure View";
		internal const string WORKERS_VIEW_TABLE_NAME = "Workers View";
		internal const string VISITORS_VIEW_TABLE_NAME = "Visitors View";

		internal const string TILES_VIEW_TABLE_NAME = "Tiles View";
		internal const string UI_VIEW_TABLE_NAME = "UI View";
		internal const string CAMERA_VIEW_TABLE_NAME = "Cameras View";
		internal const string FEEDBACK_VIEW_TABLE_NAME = "Feedback View";

		public static readonly IReadOnlyDictionary<string, string> ArchitectureToView =
			new Dictionary<string, string>()
			{
				{TableNames.ANTS_TABLE_NAME,ANTS_VIEW_TABLE_NAME },
				//{TableNames.INFTRASTRUCTURE_TABLE_NAME,INFRASTRUCTURE_VIEW_TABLE_NAME },
				//{TableNames.JAILS_TABLE_NAME,JAILS_VIEW_TABLE_NAME },
				//{TableNames.WORKERS_TABLE_NAME,WORKERS_VIEW_TABLE_NAME },
				//{TableNames.VISITORS_TABLE_NAME,VISITORS_VIEW_TABLE_NAME },
			};
	}
}