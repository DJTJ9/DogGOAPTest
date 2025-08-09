using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent (typeof(Rigidbody), typeof(GroundChecker))]
public class RigidbodyMovement : MonoBehaviour
{
    [FoldoutGroup("Settings", expanded: true), SerializeField, Range(0, 20)]
    private float speed;
    
    [FoldoutGroup("Settings", expanded: true), SerializeField, Range(0, 20)]
    private float maxSpeed;
    
    [FoldoutGroup("Settings", expanded: true), SerializeField, Range(0, 20)]
    private float jumpPower;
    
    [FoldoutGroup("Settings", expanded: true), SerializeField, Range(0, 20)]
    private float jumpSpeedModifier = 1;
    
    [FoldoutGroup("Settings", expanded: true), SerializeField, Range(0, 20)]
    private float fallSpeedModifier = 1;

    private new Transform transform;
    private Rigidbody rb;
    private AnimationControllerPlayer animations;
    private GroundChecker groundChecker;

    private Vector3 moveDirection;

    private void Awake()
    {
        transform = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        animations = GetComponent<AnimationControllerPlayer>();
        groundChecker = GetComponent<GroundChecker>();
    }

    private void FixedUpdate()
    {
        UpdateHorizontalMovement();
        UpdateVerticalMovement();
        if (rb.linearVelocity.magnitude > 0.5f) animations.SetSpeed(rb.linearVelocity.magnitude);
        else animations.SetSpeed(0f);
    }

    /// <summary>
    /// Recieves a move direction
    /// </summary>
    public void Move(Vector3 direction)
    {
        moveDirection = direction;
    }

    public void Jump()
    {
        if (groundChecker.isGrounded && rb != null)
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
    }

    /// <summary>
    /// Collects the current Velocity of the rigidbody and sets the speed
    /// Transforms moving direction from local space to world space
    /// Collects the speed difference to target velocity and clamps the max velocity
    /// Sets force mode to VelocityChange
    /// </summary>
    public void UpdateHorizontalMovement()
    {
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 targetVelocity = new Vector3(moveDirection.x, 0f , moveDirection.z);
        targetVelocity *= speed;

        targetVelocity = transform.TransformDirection(targetVelocity);

        Vector3 velocityChange = targetVelocity - currentVelocity;
        velocityChange = new Vector3(velocityChange.x, 0f, velocityChange.z);
        velocityChange = Vector3.ClampMagnitude(velocityChange, maxSpeed);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    /// <summary>
    /// Recieves the current rotation
    /// Sets the rotation to a target rotation
    /// </summary>
    public void RotateHorizontal(float rotation)
    {
        var currentRotation = rb.rotation.eulerAngles;
        var targetRotation = currentRotation + new Vector3(0f, rotation, 0f);
        rb.rotation = Quaternion.Euler(targetRotation);
    }

    /// <summary>
    /// Modifies jump and fall speed
    /// </summary>
    private void UpdateVerticalMovement()
    {
        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallSpeedModifier - 1) * Time.fixedDeltaTime;

        if (rb.linearVelocity.y > 0)
            rb.linearVelocity += Vector3.up * Physics.gravity.y * jumpSpeedModifier * Time.fixedDeltaTime;
    }
}
