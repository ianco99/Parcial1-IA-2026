
using Parcial.View.GameLogic.Entities.Instances;
using Parcial.View.Mapping;
using System;
using Parcial.Architecture.GameLogic.Entities.Instances.WarriorAnt;
using Parcial.View.GameLogic.Entities.Instances.LivingEntity;

[ViewOf(typeof(WarriorAnt))]
internal class WarriorAntView : LivingEntityView
{
    public WarriorAntView()
    {
    }

    public override Type ArchitectureEntityType => typeof(WarriorAnt);
}