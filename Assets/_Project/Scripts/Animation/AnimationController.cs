using System.Collections;
using UnityEngine;
using ImprovedTimers;
using VFavorites.Libs;

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

public abstract class AnimationController : MonoBehaviour {
    const float k_crossfadeDuration = 0.1f;
    
    Animator animator;
    CountdownTimer timer;
    
    float animationLength;
    bool dogActionEnabled;
    bool Sit_b = false;
    private float w_movement = 0.0f; 
    public float acceleration = 1.0f;
    public float deceleration = 1.0f;
    private float maxWalk = 0.5f;
    private float maxRun = 1.0f;
    private float currentSpeed;
    
    public ParticleSystem dirtFX;
    public Transform fxTransform;
    
    [HideInInspector] public int speedHash = Animator.StringToHash("Movement_f");

    
    void Awake() {
        animator = GetComponentInChildren<Animator>();
        SetSpeedHash();
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
        dogActionEnabled = true;                                 
        animator.SetInteger("ActionType_int", (int)actionType); 
        yield return new WaitForSeconds(duration);              
        animator.SetInteger("ActionType_int", 0);                
        dogActionEnabled = false;                                
    }
    
    public void SetAnimatorBool(string name, bool value) => animator.SetBool(name, value);

    public void StartDogAction(AnimationActionType actionType, float countDown = 1f) {
        StartCoroutine(DogActions(actionType, countDown));
    }

    public void SpawnDirtWhileDigging() {
        ParticleSystem go = Instantiate(dirtFX, new Vector3(transform.position.x, fxTransform.transform.position.y, fxTransform.transform.position.z), transform.rotation);
        go.transform.SetParent(fxTransform);
        go.transform.localPosition = new Vector3(go.transform.localPosition.x, go.transform.localPosition.y, go.transform.localPosition.z + 0.3f);
    }

    protected abstract void SetSpeedHash();
}

