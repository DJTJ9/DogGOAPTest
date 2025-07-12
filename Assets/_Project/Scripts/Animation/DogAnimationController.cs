using UnityEngine;

public class DogAnimationController : AnimationController {
    protected override void SetSpeedHash() {
        speedHash = Animator.StringToHash("Movement_f");
    }
}