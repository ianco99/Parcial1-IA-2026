using System;
using ianco99.ToolBox.Services;
using Parcial.Architecture.GameLogic.Entities;
using Parcial.Architecture.GameLogic.Entities.Systems;
using Parcial.Architecture.Math;
using Parcial.View.Mapping;
using Parcial.View.Scene;
using UnityEngine;

namespace Parcial.View.GameLogic.Entities.Instances
{
	[ViewOf(typeof(Entity))]
	internal abstract class EntityView : ViewComponent
	{
		private const string TEAM_MARKER_NAME = "Team marker";
		private const float TEAM_MARKER_SIZE_IN_SPRITES = 1.0f;

		protected EntityRegistry EntityRegistry => ServiceProvider.Instance.GetService<EntityRegistry>();
		protected GameScene GameScene => ServiceProvider.Instance.GetService<GameScene>();

		public abstract Type ArchitectureEntityType { get; }

		protected uint archiectureEntitiyID;
		protected int entityTeamID;
		public uint ArchitectureEnitityID => archiectureEntitiyID;
		public int EntityTeamID => entityTeamID;
		protected Entity ArchitectureEntity => EntityRegistry.GetAs<Entity>(archiectureEntitiyID);

		public static string SetIdMethodName => nameof(SetId);
		public static string SetTeamIdMethodName => nameof(SetTeamId);
		private void SetId(uint ID)
		{
			archiectureEntitiyID = ID;
		}

		private void SetTeamId(int ID)
		{
			entityTeamID = ID;
		}

		public override void Init()
		{
			base.Init();

			if (entityTeamID != Entity.UNASSIGNED_ENTITY_TEAM_ID)
				AddTeamMarker();
		}

		private void AddTeamMarker()
		{
			if (!TryGetComponent(out SpriteRenderer entitySprite) || entitySprite.sprite == null)
				return;

			GameObject markerObject = new GameObject(TEAM_MARKER_NAME);
			markerObject.transform.SetParent(transform, false);

			Vector3 spriteSize = entitySprite.sprite.bounds.size;
			float diameter = Mathf.Max(spriteSize.x, spriteSize.y) * TEAM_MARKER_SIZE_IN_SPRITES;
			markerObject.transform.localScale = new Vector3(diameter, diameter, 1.0f);

			SpriteRenderer marker = markerObject.AddComponent<SpriteRenderer>();
			marker.sprite = TeamColorsView.MarkerSprite;
			marker.color = TeamColorsView.GetColor(entityTeamID);
			marker.sortingOrder = GameScene.TEAM_MARKERS_DRAWING_ORDER;
		}

		public virtual void Move(Coordinate coordinate)
		{
			transform.position = GameScene.CoordinateToWorld(coordinate);
		}
	}

}