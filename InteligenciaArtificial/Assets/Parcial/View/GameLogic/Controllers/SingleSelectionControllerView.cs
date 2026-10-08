using System;
using System.Collections.Generic;
using Parcial.Architecture.Math;
using UnityEngine.InputSystem;

namespace Parcial.View.GameLogic.Controllers
{
	internal abstract class SingleSelectionControllerView : ControllerView
	{
		public override void Tick(float deltaTime)
		{
			Mouse mouse = Mouse.current;

			if (mouse != null && mouse.rightButton.wasPressedThisFrame)
			{
				CreateController();
			}
		}

		public override void CreateController()
		{
			Point clickPoint = GameScene.GetMouseGridCoordinate();
			List<string> blueprints = GetValidBlueprints(clickPoint);
			if (blueprints.Count == 0)
				return;

			Display(GetActionsToDisplay(clickPoint, blueprints), GetTitle(clickPoint));
		}

		protected virtual string GetTitle(Point clickPoint) => string.Empty;
		protected abstract List<string> GetValidBlueprints(Point clickPoint);
		protected abstract Dictionary<string, Action> GetActionsToDisplay(Point clickPoint, List<string> blueprints);
	}
}
