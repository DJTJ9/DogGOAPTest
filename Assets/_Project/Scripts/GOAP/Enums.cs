﻿public enum GoalType
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
    
    //Location Beliefs
    DogAtRestingPosition,
    DogAtFoodBowl,
    DogAtWaterBowl,
    DogAtRageVictim,
    DogAtBall,
    DogAtPlayer,
    
    //Sensor Beliefs
    PlayerInRange,
    PlayerInAttackRange,
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
    MoveToRageVictim,
    AttackRageVictim,
    SeekAttention,
    PickUpBall,
    DropBall,
    MoveToPlayer,
    MoveToBall,
    ReturnToPlayer,
    MoveToRestArea,
    Digging
}