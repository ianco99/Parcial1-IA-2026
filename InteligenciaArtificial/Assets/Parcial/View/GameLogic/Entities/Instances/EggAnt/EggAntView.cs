using System;
using Parcial.Architecture.GameLogic.Entities.EggAnt;
using Parcial.View.GameLogic.Entities.Instances.LivingEntity;
using Parcial.View.Mapping;

namespace Parcial.View.GameLogic.Entities.Instances
{
	[ViewOf(typeof(EggAnt))]
	internal class EggAntView : LivingEntityView
	{
		public override Type ArchitectureEntityType { get => typeof(EggAnt); }
	}
}