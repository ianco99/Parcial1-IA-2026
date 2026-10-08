using System;
using System.Collections.Generic;
using ianco99.ToolBox.Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Parcial.View.Scene
{
	internal sealed class ContextMenuView : ViewComponent, IService
	{
		public bool IsPersistance => false;

		private const float MENU_WIDTH = 200.0f;
		private const int ROW_HEIGHT = 30;

		private RectTransform container;
		private GameObject buttonPrefab;
		private Canvas canvas;

		private List<Button> spawnedRows;
		private Vector2 padding = new Vector2(10, 10);

		public override void Init()
		{
			spawnedRows = new List<Button>();
		}

		public override void Init(params object[] parameters)
		{
			base.Init(parameters);

			buttonPrefab = parameters[0] as GameObject;
			canvas = parameters[1] as Canvas;

			container = transform as RectTransform;
			container.localScale = Vector3.one;
			container.sizeDelta = Vector2.zero;
		}

		public override void Tick(float deltaTime)
		{
			Mouse mouse = Mouse.current;
			Keyboard keyboard = Keyboard.current;

			if ((mouse != null && mouse.middleButton.wasPressedThisFrame) ||
			    (keyboard != null && keyboard.escapeKey.wasPressedThisFrame))
			{
				Hide();
			}
		}

		public void Hide()
		{
			ClearAllRows();
			container.sizeDelta = Vector2.zero;
		}

		public void Show(Dictionary<string, Action> entries)
		{
			Show(null, entries);
		}

		public void Show(string title, Dictionary<string, Action> entries)
		{
			ClearAllRows();

			if (!string.IsNullOrEmpty(title))
				CreateRow(title, null).interactable = false;

			foreach (KeyValuePair<string, Action> action in entries)
			{
				CreateRow(action.Key, action.Value);
			}

			LayoutElements();
			SetPosition(Mouse.current.position.ReadValue());
		}

		private void SetPosition(Vector2 mousePosition)
		{
			RectTransform canvasRect = canvas.transform as RectTransform;

			Vector2 localPoint;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, mousePosition,
				canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out localPoint);
			container.anchoredPosition = localPoint;
		}

		private void LayoutElements()
		{
			float height = spawnedRows.Count * ROW_HEIGHT + padding.y * 2;
			container.sizeDelta = new Vector2(MENU_WIDTH, height);
			float currentY = -padding.y;

			for (int i = 0; i < spawnedRows.Count; i++)
			{
				RectTransform rect = spawnedRows[i].GetComponent<RectTransform>();

				rect.anchorMin = new Vector2(0.0f, 1.0f);
				rect.anchorMax = new Vector2(0.0f, 1.0f);
				rect.pivot = new Vector2(0.0f, 1.0f);

				rect.sizeDelta = new Vector2(MENU_WIDTH - padding.x * 2.0f, ROW_HEIGHT);
				rect.anchoredPosition = new Vector2(padding.x, currentY);

				currentY -= ROW_HEIGHT;
			}
		}

		private Button CreateRow(string name, Action action)
		{
			Button button = Instantiate(buttonPrefab, container).GetComponent<Button>();
			spawnedRows.Add(button);

			Text text = button.GetComponentInChildren<Text>();
			text.text = name;

			button.onClick.AddListener(() =>
			{
				action?.Invoke();
				Hide();
			});

			return button;
		}

		private void ClearAllRows()
		{
			for (int i = 0; i < spawnedRows.Count; i++)
			{
				spawnedRows[i].onClick.RemoveAllListeners();
				Destroy(spawnedRows[i].gameObject);
			}

			spawnedRows.Clear();
		}
	}
}
