using System;
using System.Collections.Generic;
using ianco99.ToolBox.DataFlow;
using ianco99.ToolBox.Events;
using ianco99.ToolBox.Services;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.View.GameLogic.Entities.Systems;
using Parcial.View.Scene;

namespace Parcial.View.GameLogic.Controllers
{
	internal abstract class ControllerView : ITickable, IDisposable
	{
		protected EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();
		protected GameScene GameScene => ServiceProvider.Instance.GetService<GameScene>();
		protected EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
		protected EntityRegistryView EntityRegistryView => ServiceProvider.Instance.GetService<EntityRegistryView>();
		private ContextMenuView ContextMenuView => ServiceProvider.Instance.GetService<ContextMenuView>();

		public virtual void OnSelect() { }
		public abstract void Tick(float deltaTime);

		public abstract void Dispose();

		protected void Display(Dictionary<string, Action> controls, string title = "")
		{
			ContextMenuView.Show(title, controls);
		}

		public abstract void CreateController();
	}
}
