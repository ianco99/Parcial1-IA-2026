using System;
using Parcial.Architecture.GameLogic.Entities;
using Parcial.View.GameLogic.Entities.Instances.LivingEntity;
using Parcial.View.Mapping;

namespace Parcial.View.GameLogic.Entities.Instances
{
	[ViewOf(typeof(QueenAnt))]
	internal class QueenAntView : LivingEntityView
	{
		public override Type ArchitectureEntityType { get => typeof(QueenAnt); }
				
	}
}