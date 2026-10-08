using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture.GameLogic.Controllers.ModifyTerrain;
using Parcial.Architecture.GameLogic.Controllers.ModifyTerrain.Events;
using Parcial.View.Mapping;

namespace Parcial.View.GameLogic.Controllers
{
	[ViewOf(typeof(ModifyTerrainControllerArchitecture))]
	internal sealed class ModifyTerrainControllerView
	{
		private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
		
		public ModifyTerrainControllerView()
		{
				//EventBus.Subscribe<ModifyTerrainRequestAcceptedEvent>(OnModifyTerrainRequestAccepted);
		}

		private void OnModifyTerrainRequestAccepted(in ModifyTerrainRequestAcceptedEvent callback)
		{
			
		}
	}
}