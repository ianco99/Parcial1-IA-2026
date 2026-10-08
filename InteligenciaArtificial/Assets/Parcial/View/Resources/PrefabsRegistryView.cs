using System.Collections.Generic;
using System.IO;
using ianco99.ToolBox.Blueprints;
using ianco99.ToolBox.Services;
using Parcial.Architecture.Console;
using UnityEngine;

namespace Parcial.View.Resources
{
	public class PrefabsRegistryView : IService
	{
		private BlueprintRegistry BlueprintRegistry => ServiceProvider.Instance.GetService<BlueprintRegistry>();
		private BlueprintBinder BlueprintBinder => ServiceProvider.Instance.GetService<BlueprintBinder>();

		public bool IsPersistance => false;

		private Dictionary<string, Dictionary<string, string>> prefabPaths;
		private Dictionary<string, Dictionary<string, string>> architectureToViewId;
		private Dictionary<string, Dictionary<string, GameObject>> prefabs;

		private GameObject missingPrefab;

		public PrefabsRegistryView()
		{
			prefabs = new Dictionary<string, Dictionary<string, GameObject>>();
			prefabPaths = new Dictionary<string, Dictionary<string, string>>();
			architectureToViewId = new Dictionary<string, Dictionary<string, string>>();
			missingPrefab = UnityEngine.Resources.Load<GameObject>(Path.Combine("Prefabs", "MissingPrefab"));

			foreach (string prefabFile in TableNamesView.PREFAB_TABLES)
			{
				prefabs.Add(prefabFile, new Dictionary<string, GameObject>());
				prefabPaths.Add(prefabFile, new Dictionary<string, string>());
				architectureToViewId.Add(prefabFile, new Dictionary<string, string>());

				foreach (string id in BlueprintRegistry.BlueprintsOf(prefabFile))
				{
					object prefabPath = new PrefabPath();
					BlueprintBinder.Apply(ref prefabPath, prefabFile, id);
					prefabPaths[prefabFile].Add(((PrefabPath)prefabPath).architectureID, ((PrefabPath)prefabPath).PrefabResourcePath);
					architectureToViewId[prefabFile].Add(((PrefabPath)prefabPath).architectureID, id);
				}
			}
		}

		public string ArchitectureToViewId(string tableName, string architectureID)
		{
			return architectureToViewId[tableName][architectureID];
		}

		public GameObject Get(string tableName, string architectureID)
		{
			string resourcePath = prefabPaths[tableName][architectureID];

			if (prefabs[tableName].ContainsKey(resourcePath))
				return prefabs[tableName][resourcePath];
			
			GameObject prefab = UnityEngine.Resources.Load<GameObject>(resourcePath);

			if (prefab == null)
			{
				prefab = missingPrefab;
				GameConsole.Warning($"Missing prefab in resource folder: {resourcePath}");
			}
			
			prefabs[tableName].Add(resourcePath, prefab);
			return prefab;
		}
		
		private struct PrefabPath
		{
			[BlueprintParameter("Architecture ID")] public string architectureID;
			[BlueprintParameter("Prefab resource path")] private string prefabResourcePath;

			public string PrefabResourcePath => prefabResourcePath.Replace('/', Path.DirectorySeparatorChar);
		}
	}
}