using UnityEngine;

public class AnimationControllerPlayer : MonoBehaviour {
    const float k_crossfadeDuration = 0.1f;
    
    private Animator animator;
    
    [HideInInspector] public int speedHash = Animator.StringToHash("Speed");
    [HideInInspector] public int locoMotionHash = Animator.StringToHash("LocoMotion");
    [HideInInspector] public int deathBoolHash = Animator.StringToHash("isDead");
    [HideInInspector] public int isKickingHash = Animator.StringToHash("isKicking");
    [HideInInspector] public int isPunchingLeftHash = Animator.StringToHash("isPunching_Left");
    [HideInInspector] public int isPunchingRightHash = Animator.StringToHash("isPunching_Right");
    [HideInInspector] public int isWavingHash = Animator.StringToHash("isWaving");
    [HideInInspector] public int isTextingHash = Animator.StringToHash("isTexting");
    
    void Awake() {
        animator = GetComponentInChildren<Animator>();
    }

    public void SetSpeed(float speed) => animator.SetFloat(speedHash, speed);
    
    public void SetAnimatorBool(string name, bool value) => animator.SetBool(name, value);
    
    public void LocoMotion() {
        animator.CrossFade(locoMotionHash, k_crossfadeDuration);
    }
}