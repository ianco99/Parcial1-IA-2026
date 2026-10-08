using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities.EggAnt
{
	public class EggAnt : LivingEntity.LivingEntity
	{
		public const int EGG_ANT_HEALTH = 20;
		public const int EGG_ANT_SPEED = 1;
		public const int EGG_ANT_MOVE_TIME = 1;
		public const float HATCH_TIME = 20f;

		public enum HatchTypes
		{
			BuilderAnt,
			WarriorAnt
		}

		public HatchTypes HatchType;

		protected EggAnt(uint ID, Coordinate coordinate, int teamID) : base(ID, coordinate, teamID, EGG_ANT_HEALTH, EGG_ANT_SPEED, EGG_ANT_MOVE_TIME)
		{
			
		}
	}
}