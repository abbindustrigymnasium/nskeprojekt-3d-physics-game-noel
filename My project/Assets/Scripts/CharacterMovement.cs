using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using static UnityEngine.Random;

public class CharacterMovement : MonoBehaviour {
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float wallRunSpeed;
    public float slideSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Camera Effects")]
    public PlayerCam cam;
    public float grappleFov = 95f;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    public Rigidbody player;

    public MovementState mState;
    public enum MovementState {
        freeze,
        walking,
        sprinting,
        crouching,
        wallRunning,
        grappling,
        sliding,
        air
    }

    public bool sliding;
    public bool freeze;
    public bool activeGrapple;
    public bool wallRunning;

    private void Start() {

        player = GetComponent<Rigidbody>();
        player.freezeRotation = true;
        readyToJump = true;

        startYScale = transform.localScale.y;

    }

    private void Update() {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();
        if (grounded && !activeGrapple) {
            player.drag = groundDrag;
        }
        else {
            player.drag = 0;
        }
    }
    private void FixedUpdate() {
        MovePlayer();
    }

    private void MyInput() {

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded) {

            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey)) {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            player.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        if (OnSlope() && Input.GetKey(crouchKey)) {
            player.AddForce(Vector3.down * 200f, ForceMode.Acceleration);
        }

        if (Input.GetKeyUp(crouchKey)) {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler() {

        // Mode - Wallrunning
        if (wallRunning) {
            mState = MovementState.wallRunning;
            desiredMoveSpeed = wallRunSpeed;
        }

        // Mode - Sliding
        else if (sliding) {
            mState = MovementState.sliding;

            if (OnSlope() && player.velocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;
            else
                desiredMoveSpeed = sprintSpeed;
        }

        // Mode - Freeze
        else if (freeze) {
            mState = MovementState.freeze;
            moveSpeed = 0;
            player.velocity = Vector3.zero;
        }

        // Mode - Grappling
        else if (activeGrapple) {
            mState = MovementState.grappling;
            moveSpeed = sprintSpeed;
        }

        // Mode - Crouching
        else if (Input.GetKey(crouchKey)) {
            mState = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        // Mode - Sprinting
        else if (grounded && Input.GetKey(sprintKey)) {
            mState = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }

        // Mode - Walking
        else if (grounded) {
            mState = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // Mode - Air
        else {
            mState = MovementState.air;
        }

        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > (sprintSpeed - walkSpeed)) {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        } 
        else {
            moveSpeed = desiredMoveSpeed;
        }
        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed() {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference) {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            time += Time.deltaTime;
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer() {

        if (activeGrapple) return;

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope) {
            player.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);
            if (player.velocity.y > 0) {
                player.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        else if (grounded)
            player.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if (!grounded)
            player.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        if (!wallRunning) player.useGravity = !OnSlope();
    }

    private void SpeedControl() {

        if (activeGrapple) return;

        if (OnSlope() && !exitingSlope) {
            if (player.velocity.magnitude > moveSpeed)
                player.velocity = player.velocity.normalized * moveSpeed;
            
        }
        else {
            Vector3 flatVel = new Vector3(player.velocity.x, 0f, player.velocity.z);

            if (flatVel.magnitude > moveSpeed) {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                player.velocity = new Vector3(limitedVel.x, player.velocity.y, limitedVel.z);
            }
        }

    }

    private void Jump() {
        exitingSlope = true;
        player.velocity = new Vector3(player.velocity.x, 0f, player.velocity.z);
        player.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump() {
        readyToJump = true;
        exitingSlope = false;
    }

    private bool enableMovementOnNextTouch;
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight) {
        activeGrapple = true;
        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);
        Invoke(nameof(ResetRestrictions), 3f);
    }

    private Vector3 velocityToSet;
    private void SetVelocity() {
        enableMovementOnNextTouch = true;
        player.velocity = velocityToSet;

        cam.DoFov(grappleFov);
    }

    public void ResetRestrictions() {
        activeGrapple = false;
        cam.DoFov(80f);
    }

    private void OnCollisionEnter(Collision collision) {
        if (enableMovementOnNextTouch) {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            GetComponent<Grappling>().StopGrapple();
        }
    }

    public bool OnSlope() {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f)) {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction) {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight) {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
        + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));
        return velocityXZ + velocityY;
    }
}
