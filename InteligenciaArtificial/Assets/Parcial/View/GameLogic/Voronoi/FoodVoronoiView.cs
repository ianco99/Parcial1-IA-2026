using Parcial.Architecture.GameLogic.Voronoi.Events;
using Parcial.View.Scene;
using UnityEngine;

namespace Parcial.View.GameLogic.Voronoi
{
    internal sealed class FoodVoronoiView : VoronoiView
    {
        protected override Color SegmentColor => Color.white;
        protected override Color VertexColor => Color.yellow;
        protected override float LineWidthInCells => 0.12f;
        protected override int DrawingOrder => GameScene.ENTITIES_DRAWING_ORDER + 2;

        public override void Init()
        {
            base.Init();
            EventBus.SubscribeOnlyMainThread<FoodVoronoiRecalculatedEvent>(OnFoodVoronoiRecalculated);
        }

        private void OnFoodVoronoiRecalculated(in FoodVoronoiRecalculatedEvent foodVoronoiRecalculatedEvent)
        {
            Draw(foodVoronoiRecalculatedEvent.diagram);
        }

        public override void Dispose()
        {
            base.Dispose();
            EventBus.UnsubscribeOnlyMainThread<FoodVoronoiRecalculatedEvent>(OnFoodVoronoiRecalculated);
        }
    }
}
