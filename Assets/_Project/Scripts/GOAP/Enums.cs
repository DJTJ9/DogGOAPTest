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
    FoodIsAvailable,
    WaterIsAvailable,
    RestingSpotIsAvailable,
    
    //Location Beliefs
    DogAtRestingPosition,
    DogAtFoodBowl,
    DogAtWaterBowl,
    DogAtObstacle1,
    DogAtBall,
    DogAtPlayer,
    
    //Sensor Beliefs
    PlayerInRange,
    PlayerInAttackRange,
    DogAtPlayerWithBall,
    WaitingForBall,
    DogAtObstacle2,
    DogAtObstacle3
}

public enum ActionType
{
    Relax,
    WanderAround,
    FetchBall,
    MoveToEatingPosition,
    Eat,
    MoveToDrinkingPosition,
    Drink,
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
    AttackObstacle3
}