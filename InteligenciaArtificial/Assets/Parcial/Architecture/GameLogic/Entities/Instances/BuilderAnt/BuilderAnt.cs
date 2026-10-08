using KuznickiAgents.StatePattern.BehaviourTree;
using Parcial.Architecture.Console;
using Parcial.Architecture.GameLogic.Entities.Nodes;
using Parcial.Architecture.Math;

namespace Parcial.Architecture.GameLogic.Entities
{
	public class BuilderAnt : LivingEntity.LivingEntity
	{
		public const int BUILDER_ANT_HEALTH = 20;
		public const int BUILDER_ANT_SPEED = 1;
		public const float BUILDER_ANT_MOVE_TIME = 0.1f;
		
		public const string ENTITY_ID_KEY = "entityID";
		public const string ENTITY_SPEED_KEY= "entitySpeed";
		public const string FOOD_ID_KEY = "foodID";
		public const string QUEEN_ID_KEY = "queenID";
		public const string TARGET_KEY = "target";
		public const string CLAIM_TILE_KEY = "claimTile";
		public const string LOGS_ENABLED_KEY = "logsEnabled";

		private BehaviourTree behaviourTree;

		private bool hasFood = false;
		private uint foodID = default;
		private int currentTarget = (int)Targets.None;

		public bool HasFood => hasFood;

		public enum Targets
		{
			QueenAnt,
			Food,
			Tile,
			None
		}



		protected BuilderAnt(uint ID, Coordinate coordinate, int teamID) : base(ID, coordinate, teamID, BUILDER_ANT_HEALTH, BUILDER_ANT_SPEED, BUILDER_ANT_MOVE_TIME)
		{

		}

		public override void Init()
		{
			base.Init();

			LeaveFoodAction leaveFoodAction = new LeaveFoodAction();
			HasFoodCondition foodToDrop = new HasFoodCondition(leaveFoodAction, () => this.hasFood);
			ForceSuccessDecoratorNode dropFood = new ForceSuccessDecoratorNode(foodToDrop);

			SetTargetToClaimTileAction setTargetToClaimTile = new SetTargetToClaimTileAction();
			CanClaimTileCondition canClaimTile = new CanClaimTileCondition(setTargetToClaimTile, teamID);

			ChangeCurrentTargetValueAction forgetQueen = new ChangeCurrentTargetValueAction(TARGET_KEY, (int)Targets.None);
			SelectorNode claimOrSearch = new SelectorNode(canClaimTile, forgetQueen);

			SequenceNode dropThenDecide = new SequenceNode(dropFood, claimOrSearch);


			//QUEEN		Approach queen 
			MoveToPointAction moveToQueen = new MoveToPointAction();
			//

			IsCloseToTargetCondition isCloseToQueenCondition = new IsCloseToTargetCondition(dropThenDecide, ID, QUEEN_ID_KEY);
				
			//QUEEN		target queen
			SelectorNode moveOrRangeToDropFood = new SelectorNode(isCloseToQueenCondition, moveToQueen);
			//
			
			
			
			//FOOD		Collect or filled
			
			CollectFoodAction collectFoodAction = new CollectFoodAction();
			
			
			ChangeCurrentTargetValueAction changeToQueen = new ChangeCurrentTargetValueAction(TARGET_KEY, (int)Targets.QueenAnt);
			CreatePathAction createPathToQueen = new CreatePathAction(QUEEN_ID_KEY);
			SequenceNode targetQueenSequence = new SequenceNode(changeToQueen, createPathToQueen);
			HasFoodCondition hasFood = new HasFoodCondition(targetQueenSequence, () => this.hasFood);
			//

			//FODD		Close to food
			IsCloseToTargetCondition isCloseToFoodCondition = new IsCloseToTargetCondition(collectFoodAction, ID, FOOD_ID_KEY);
			//
			
			//FOOD		Approach food
			MoveToPointAction moveToFood = new MoveToPointAction();
			//

			//FOOD		Target food
			ChangeCurrentTargetValueAction forgetFood = new ChangeCurrentTargetValueAction(TARGET_KEY, (int)Targets.None);

			SelectorNode moveOrRangeToEat = new SelectorNode(hasFood, isCloseToFoodCondition, moveToFood, forgetFood);
			//

			MoveToPointAction moveToTile = new MoveToPointAction();
			ClaimTileAction claimTileAction = new ClaimTileAction();
			SequenceNode claimTileSequence = new SequenceNode(moveToTile, claimTileAction, targetQueenSequence);

			SelectorNode moveOrClaimTile = new SelectorNode(claimTileSequence, targetQueenSequence);

			//Select target
			IsCurrentTargetCondition queenTargetCondition = new IsCurrentTargetCondition(moveOrRangeToDropFood,(int)Targets.QueenAnt);

			IsCurrentTargetCondition foodTargetCondition =
				new IsCurrentTargetCondition(moveOrRangeToEat, (int)Targets.Food);

			IsCurrentTargetCondition tileTargetCondition =
				new IsCurrentTargetCondition(moveOrClaimTile, (int)Targets.Tile);
			//

			SetUpBlackboardAction setUpBlackboard = new SetUpBlackboardAction(ID, teamID, Speed, foodID, currentTarget);

			SetTargetToFoodAction setTargetToFood = new SetTargetToFoodAction();
			CreatePathAction createPathToFood = new CreatePathAction(FOOD_ID_KEY);
			SequenceNode targetFoodSequence = new SequenceNode(setTargetToFood, createPathToFood);
			IsCurrentTargetCondition noTargetCondition = new IsCurrentTargetCondition(targetFoodSequence, (int)Targets.None);

			SelectorNode targetSelector = new SelectorNode(noTargetCondition, foodTargetCondition, tileTargetCondition, queenTargetCondition);

			SequenceNode targetSequence = new SequenceNode(setUpBlackboard, targetSelector);

			RootNode root = new RootNode(targetSequence);

			behaviourTree = new BehaviourTree(root);
		}

		public override void Tick(float deltaTime)
		{
			base.Tick(deltaTime);

			behaviourTree.Tick();
		}

		public static bool LogsEnabled(Blackboard blackboard)
		{
			return blackboard.TryGetData(LOGS_ENABLED_KEY, out object data) && (bool)data;
		}

		public static void Log(Blackboard blackboard, string message)
		{
			if (LogsEnabled(blackboard))
				GameConsole.Log(message);
		}

		public void CollectFood(uint foodID)
		{
			hasFood = true;
			this.foodID = foodID;
		}

		public void DropFood()
		{
			hasFood = false;
			this.foodID = default;
		}
	}
}