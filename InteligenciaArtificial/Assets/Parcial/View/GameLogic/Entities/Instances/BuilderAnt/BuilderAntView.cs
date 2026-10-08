using Parcial.View.Mapping;
using System;
using Parcial.View.GameLogic.Entities.Instances.LivingEntity;

namespace Parcial.View.GameLogic.Entities.Instances.BuilderAnt
{
	[ViewOf(typeof(Parcial.Architecture.GameLogic.Entities.BuilderAnt))]
	internal class BuilderAntView : LivingEntityView
	{
		public override Type ArchitectureEntityType => typeof(Architecture.GameLogic.Entities.BuilderAnt);
	}
}