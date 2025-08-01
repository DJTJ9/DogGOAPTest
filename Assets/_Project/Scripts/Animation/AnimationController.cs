using System.Collections;
using UnityEngine;
using ImprovedTimers;

public enum AnimationActionType
{
    NoAction = 0,
    Bark,
    Beg = 2,
    Cower,
    Dig,
    Eat,
    Howl,
    Drink,
    Pee,
    Poo,
    Shake,
    Sniff,
    Yawn,
    ShakeToy
}

public class AnimationController : MonoBehaviour {
    const float k_crossfadeDuration = 0.1f;
    
    private Animator animator;
    private CountdownTimer timer;
    private float animationLength;
    // bool dogActionEnabled;
    // bool Sit_b = false;
    // private float w_movement = 0.0f; 
    // public float acceleration = 1.0f;
    // public float deceleration = 1.0f;
    // private float maxWalk = 0.5f;
    // private float maxRun = 1.0f;
    private float currentSpeed;
    
    public Transform fxTransform;
    public ParticleSystem dirtFX;
    public ParticleSystem pooFX;
    public ParticleSystem peeFX;
    public ParticleSystem splashWaterFX;
    
    
    [HideInInspector] public int speedHash = Animator.StringToHash("Movement_f");
    [HideInInspector] public int locoMotionHash = Animator.StringToHash("Locomotion");
    [HideInInspector] public int deathBoolHash = Animator.StringToHash("Death_b");

    
    void Awake() {
        animator = GetComponentInChildren<Animator>();
    }

    public void SetSpeed(float speed) => animator.SetFloat(speedHash, speed);
    
    void Update() => timer?.Tick(Time.deltaTime);

    #region Helper Methods
    // void PlayAnimationUsingTimer(int clipHash) {
    //     timer = new CountdownTimer(GetAnimationLength(clipHash));
    //     timer.OnTimerStart += () => animator.CrossFade(clipHash, k_crossfadeDuration);
    //     timer.OnTimerStop += () => animator.CrossFade(locomotionClip, k_crossfadeDuration);
    //     timer.Start();
    // }
    //
    // public float GetAnimationLength(int hash) {
    //     if (animationLength > 0) return animationLength;
    //
    //     foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips) {
    //         if (Animator.StringToHash(clip.name) == hash) {
    //             animationLength = clip.length;
    //             return clip.length;
    //         }
    //     }
    //
    //     return -1f;
    // }
    #endregion
    
    public IEnumerator DogActions(AnimationActionType actionType, float duration = 1f)
    {
        // dogActionEnabled = true;                                 
        animator.SetInteger("ActionType_int", (int)actionType); 
        yield return new WaitForSeconds(duration);              
        animator.SetInteger("ActionType_int", 0);                
        // dogActionEnabled = false;                                
    }

    public IEnumerator AttackAction(int attackType, float readyDuration = 2f) {
        animator.SetBool("AttackReady_b", true);
        yield return new WaitForSeconds(readyDuration);        
        animator.SetInteger("AttackType_int", attackType);
        yield return new WaitForSeconds(1f);
        animator.SetInteger("AttackType_int", 0);
        animator.SetBool("AttackReady_b", false);
    }
    
    public void SetAnimatorBool(string name, bool value) => animator.SetBool(name, value);

    public void StartDogAction(AnimationActionType actionType, float countDown = 1f) {
        StartCoroutine(DogActions(actionType, countDown));
    }

    public void StartAttackAction() {
        StartCoroutine(AttackAction(3));
    }

    public void LocoMotion() {
        animator.CrossFade(locoMotionHash, k_crossfadeDuration);
    }

    public void SpawnDirtWhileDigging() {
        ParticleSystem go = Instantiate(dirtFX, new Vector3(transform.position.x, fxTransform.transform.position.y, fxTransform.transform.position.z), transform.rotation);
        go.transform.SetParent(fxTransform);
        go.transform.localPosition = new Vector3(go.transform.localPosition.x, go.transform.localPosition.y, go.transform.localPosition.z + 0.3f);
    }
}

