using UnityEngine;

public class CactusAnimationController : AnimationController {
    protected override void SetLocomotionClip() {
        locomotionClip = Animator.StringToHash("Locomotion");
    }
    
    protected override void SetAttackClip() {
        attackClip = Animator.StringToHash("Attack");
    }
    
    protected override void SetSpeedHash() {
        speedHash = Animator.StringToHash("Speed");
    }

    protected override void SetEatClip() {
        eatClip = Animator.StringToHash("Eat");   
    }
    
    protected override void SetDrinkClip() {
        drinkClip = Animator.StringToHash("Drink");   
    }
}