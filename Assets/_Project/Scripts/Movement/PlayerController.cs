using System;
using ScriptableValues;
using UnityEngine;
using UnityEngine.InputSystem;
// using BlackboardSystem;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private RigidbodyMovement rigidbodyMovement;

    [SerializeField]
    private CameraRotator cameraRotator;

    [Header("Interact")]
    [SerializeField]
    private Transform mainCameraTransform;

    [SerializeField]
    private Transform objectGrabPoint;

    private GrabbableObject grabbableObject;

    [Header("Input")]
    [SerializeField]
    private PlayerInput playerInput;

    [Header("Settings")]
    [SerializeField]
    private float lookSensitivity = 0.15f;

    [Header("Scriptable Values"), SerializeField]
    private ScriptableBoolValue ballInHand;

    [SerializeField]
    private ScriptableBoolValue ballThrown;

    [SerializeField]
    private ScriptableBoolValue ballReturned;

    [SerializeField]
    private ScriptableBoolValue dogCalled;

    private AnimationControllerPlayer animations;
   
    private InputAction moveInputAction;
    private InputAction jumpInputAction;
    private InputAction lookInputAction;
    private InputAction interactionInputAction;
    private InputAction dropInputAction;
    private InputAction throwInputAction;
    private InputAction callDogInputAction;
    private InputAction applicationQuitInputAction;

    private const float interactionDistance = 5f;

    private void Awake() {
        MapInputActions();
        animations = GetComponent<AnimationControllerPlayer>();
    }

    /// <summary>
    /// Sets cursor lock mode on left click to locked and on escape to none.
    /// Gets move direction from input and moves rigidbody into this direction.
    /// Rotates the rigidbody horizontally if cursor lock mode is locked.
    /// </summary>
    private void Update() {
        if (Mouse.current.rightButton.wasPressedThisFrame) Cursor.lockState = CursorLockMode.Locked;
        if (Keyboard.current.escapeKey.wasPressedThisFrame) Cursor.lockState = CursorLockMode.None;


        var moveDirection = GetMoveDirectionFromInput();
        rigidbodyMovement.Move(moveDirection);

        if (Cursor.lockState == CursorLockMode.Locked) {
            var rotation = GetRotationFromInput();
            rigidbodyMovement.RotateHorizontal(rotation.x * lookSensitivity);
        }

        if (Input.GetKeyDown(KeyCode.Q)) {
            // blackboard.Debug();
        }
    }

    /// <summary>
    /// Rotates camera vertically if cursor lock mode is locked.
    /// </summary>
    private void LateUpdate() {
        if (Cursor.lockState == CursorLockMode.Locked) {
            if (cameraRotator != null)
                UpdateCamera();
        }
    }

    /// <summary>
    /// Gets rotation from input
    /// Rotates camera in the direction of the rotation input
    /// </summary>
    private void UpdateCamera() {
        var rotation = GetRotationFromInput();
        cameraRotator.Rotate(rotation.y);
    }

    /// <summary>
    /// Maps the input actions
    /// Subcribes the OnJumpInput method to the jump started action
    /// </summary>
    private void MapInputActions() {
        moveInputAction = playerInput.actions["Move"];

        jumpInputAction = playerInput.actions["Jump"];
        jumpInputAction.started += OnJumpInput;

        lookInputAction = playerInput.actions["Look"];

        interactionInputAction = playerInput.actions["Interact"];
        interactionInputAction.started += OnInteractInput;
        interactionInputAction.started += OnStatusRequstInput;
        interactionInputAction.started += OnGrabInput;

        dropInputAction = playerInput.actions["Drop"];
        dropInputAction.started += OnDropInput;

        throwInputAction = playerInput.actions["Throw"];
        throwInputAction.started += OnThrowInput;

        callDogInputAction = playerInput.actions["Call Dog"];
        callDogInputAction.started += OnCallDogInput;

        applicationQuitInputAction = playerInput.actions["Quit"];
        applicationQuitInputAction.started += context => OnApplicationQuit();
    }

    private void OnJumpInput(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started)
            rigidbodyMovement.Jump();
    }

    /// <summary>
    /// Gets the horizontal move direction from the input
    /// Converts this input into a 3D vector and returns it
    /// </summary>
    private Vector3 GetMoveDirectionFromInput() {
        var moveInput = moveInputAction.ReadValue<Vector2>();
        return new Vector3(moveInput.x, 0f, moveInput.y);
    }

    /// <summary>
    /// Gets the rotation input and returns it
    /// </summary>
    private Vector2 GetRotationFromInput() {
        return lookInputAction.ReadValue<Vector2>();
    }

    private void OnInteractInput(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started) {
            if (Physics.Raycast(mainCameraTransform.position, mainCameraTransform.forward, out RaycastHit raycastHit, interactionDistance)) {
                if (raycastHit.transform.TryGetComponent(out IInteractable interactable)) {
                    interactable.Interact();
                    Debug.Log($"Player interacted with {interactable.GetType().Name}");
                }
            }
        }
    }

    private void OnStatusRequstInput(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started) {
            if (Physics.Raycast(mainCameraTransform.position, mainCameraTransform.forward, out RaycastHit raycastHit, interactionDistance)) {
                if (raycastHit.transform.TryGetComponent(out DogStatus interactable)) {
                    interactable.Interact();
                }
            }
        }
    }

    private void OnGrabInput(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started)
            if (grabbableObject == null) {
                if (Physics.Raycast(mainCameraTransform.position, mainCameraTransform.forward,
                        out RaycastHit raycastHit, interactionDistance)) {
                    if (raycastHit.transform.TryGetComponent(out GrabbableObject grabbableObj)) {
                        grabbableObj.Grab(objectGrabPoint);
                        grabbableObj.GetComponent<CapsuleCollider>().enabled = false;
                        grabbableObj.objectPossessed = true;
                        grabbableObject = grabbableObj;
                        ballThrown.Value = false;
                        ballInHand.Value = true;
                        ballReturned.Value = false;
                    }
                }
            }
    }

    private void OnDropInput(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started) {
            grabbableObject.Drop();
            grabbableObject.objectPossessed = false;
            grabbableObject = null;
            ballInHand.Value = false;
            ballReturned.Value = true;
        }
    }

    private void OnThrowInput(InputAction.CallbackContext context) {
        if (grabbableObject == null) return;

        if (context.phase == InputActionPhase.Started) {
            animations.SetAnimatorTrigger("isThrowing");
            grabbableObject.GetComponent<CapsuleCollider>().enabled = true;
            grabbableObject.Throw(mainCameraTransform.forward);
            grabbableObject.objectPossessed = false;
            grabbableObject = null;
            ballThrown.Value = true;
            ballInHand.Value = false;
            
        }
    }

    private void OnCallDogInput(InputAction.CallbackContext context) {
        if (context.phase == InputActionPhase.Started) {
            if (Physics.Raycast(mainCameraTransform.position, mainCameraTransform.forward, out RaycastHit raycastHit, 15f)) {
                if (raycastHit.transform.TryGetComponent(out ICommandable commandable)) {
                    commandable.ExecuteCommand();
                }
            }
        }
        dogCalled.Value = !dogCalled.Value;
    }

    private void OnApplicationQuit() {
        // #if UNITY_EDITOR
        // UnityEditor.EditorApplication.isPlaying = false;
        // #endif

        Application.Quit();
    }
}