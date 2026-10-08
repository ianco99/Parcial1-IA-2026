using System;
using System.Collections.Generic;
using ianco99.ToolBox.DataFlow;
using UnityEngine.InputSystem;

namespace Parcial.View.GameLogic.Controllers
{
	internal sealed class ControllerViewStrategy : ITickable, IDisposable
	{
		private Dictionary<Type, ControllerView> controllers;

		private Type currentStrategyType;

		private IReadOnlyDictionary<Key, Type> StrategyBinder { get; } =
			new Dictionary<Key, Type>()
			{
				{ Key.Digit1, typeof(ChangePathfindingAlgorithmControllerView) },
			};

		public ControllerViewStrategy()
		{
			controllers = new Dictionary<Type, ControllerView>();
			currentStrategyType = typeof(ChangePathfindingAlgorithmControllerView);
			controllers.Add(typeof(ChangePathfindingAlgorithmControllerView), new ChangePathfindingAlgorithmControllerView());
		}

		public void Tick(float deltaTime)
		{
			Keyboard keyboard = Keyboard.current;

			if (keyboard != null)
			{
				foreach (KeyValuePair<Key, Type> strategyBinding in StrategyBinder)
				{
					if (keyboard[strategyBinding.Key].wasPressedThisFrame)
					{
						currentStrategyType = strategyBinding.Value;
						controllers[currentStrategyType].OnSelect();
					}
				}
			}

			controllers[currentStrategyType].Tick(deltaTime);
		}

		public void Dispose()
		{
			foreach (KeyValuePair<Type, ControllerView> controller in controllers)
			{
				controller.Value.Dispose();
			}
		}
	}
}
