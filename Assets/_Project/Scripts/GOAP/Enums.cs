public enum GoalType
{
    Idle,
    Wander,
    KeepThirstLow,
    KeepHungerLow,
    KeepExhaustionLow,
    Attack,
    KeepBoredomLow,
    FetchBallAndReturnIt,
    StayAlive
}

public enum Beliefs {
    Nothing,
    Idle,
    IsMoving,
    DogIsExhausted,
    DogIsRested,
    DogIsNotHungry,
    DogIsHungry,
    DogIsThirsty,
    DogIsNotThirsty,
    DogIsHappy,
    DogIsBored,
    BallInHand,
    BallThrown,
    Attacking,
    AttackingRageVictim,
    BallReturned,
    ReturnBall,
    FetchBall,
    DropBall,
    
    //Global Beliefs
    FoodBowl1IsAvailable,
    WaterBowl1IsAvailable,
    RestingSpotIsAvailable,
    
    //Location Beliefs
    DogAtRestingPosition,
    DogAtFoodBowl1,
    DogAtWaterBowl1,
    DogAtObstacle1,
    DogAtBall,
    DogAtPlayer,
    
    //Sensor Beliefs
    PlayerInRange,
    PlayerInAttackRange,
    DogAtPlayerWithBall,
    WaitingForBall,
    DogAtObstacle2,
    DogAtObstacle3,
    DogAtFoodBowl2,
    DogAtWaterBowl2,
    FoodBowl2IsAvailable,
    WaterBowl2IsAvailable,
    DogAtObstacle4
}

public enum ActionType
{
    Relax,
    WanderAround,
    FetchBall,
    MoveToFoodBowl1,
    EatAtBowl1,
    MoveToWaterBowl1,
    DrinkAtBowl1,
    Sleep,
    Rest,
    MoveToObstacle1,
    AttackObstacle1,
    SeekAttention,
    PickUpBall,
    DropBall,
    MoveToPlayer,
    MoveToBall,
    ReturnToPlayer,
    MoveToRestArea,
    Digging,
    MoveToPlayerWithBall,
    WaitForBallThrow,
    MoveToObstacle2,
    AttackObstacle2,
    MoveToObstacle3,
    AttackObstacle3,
    MoveToFoodBowl2,
    MoveToObstacle4,
    AttackObstacle4,
    MoveToWaterBowl2,
    EatAtBowl2,
    DrinkAtBowl2
}