using ianco99.ToolBox.Services;
using Parcial.Architecture.Config;
using UnityEngine;

namespace Parcial.View
{
	internal static class TeamColorsView
	{
		private const float TEAM_SATURATION = 1.0f;
		private const float TEAM_VALUE = 1.0f;

		private const int MARKER_TEXTURE_SIZE = 64;
		private const float MARKER_RIM_WIDTH_IN_PIXELS = 5.0f;

		private static readonly Color NO_TEAM_COLOR = Color.white;
		private static readonly Color MARKER_RIM_COLOR = new Color(0.1f, 0.1f, 0.1f);

		private static MapConfigReader MapConfigReader => ServiceProvider.Instance.GetService<MapConfigReader>();

		private static Sprite markerSprite;

		public static Sprite MarkerSprite
		{
			get
			{
				if (markerSprite == null)
					markerSprite = CreateMarkerSprite();

				return markerSprite;
			}
		}

		public static Color GetColor(int teamId)
		{
			if (teamId < 0)
				return NO_TEAM_COLOR;

			float hue = Mathf.Repeat((float)teamId / MapConfigReader.AnthillAmount, 1.0f);

			return Color.HSVToRGB(hue, TEAM_SATURATION, TEAM_VALUE);
		}

		private static Sprite CreateMarkerSprite()
		{
			Texture2D texture = new Texture2D(MARKER_TEXTURE_SIZE, MARKER_TEXTURE_SIZE, TextureFormat.RGBA32, false);
			texture.wrapMode = TextureWrapMode.Clamp;

			float radius = MARKER_TEXTURE_SIZE * 0.5f;
			Vector2 center = new Vector2(radius, radius);

			for (int x = 0; x < MARKER_TEXTURE_SIZE; x++)
			{
				for (int y = 0; y < MARKER_TEXTURE_SIZE; y++)
				{
					float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);

					Color pixel = Color.Lerp(MARKER_RIM_COLOR, Color.white,
						Mathf.Clamp01(radius - MARKER_RIM_WIDTH_IN_PIXELS - distance));
					pixel.a = Mathf.Clamp01(radius - distance);

					texture.SetPixel(x, y, pixel);
				}
			}

			texture.Apply();

			return Sprite.Create(texture, new Rect(0.0f, 0.0f, MARKER_TEXTURE_SIZE, MARKER_TEXTURE_SIZE),
				new Vector2(0.5f, 0.5f), MARKER_TEXTURE_SIZE);
		}
	}
}
