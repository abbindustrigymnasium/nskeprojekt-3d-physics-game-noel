using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;

public class CharacterMovement : MonoBehaviour {
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float wallRunSpeed;

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

    public MovementState movementState;
    public enum MovementState {
        freeze,
        walking,
        sprinting,
        crouching,
        wallRunning,
        air
    }
    public bool freeze;
    public bool activeGrapple;
    public bool wallRunning;
    // private bool isMoving = false;
    // private bool isRunning = false;

    private void Start() {

        // Physics.gravity = new Vector3(0, -3.0f, 0);
        player = GetComponent<Rigidbody>();
        player.freezeRotation = true;
        readyToJump = true;

        startYScale = transform.localScale.y;

    }

    // Update is called once per frame
    private void Update() {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        if (grounded && !activeGrapple) {
            player.drag = groundDrag;
        } else {
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


        if (wallRunning) {
            movementState = MovementState.wallRunning;
            moveSpeed = wallRunSpeed;
        }
        if (freeze) {
            movementState = MovementState.freeze;
            moveSpeed = 0;
            player.velocity = Vector3.zero;
        } else if (Input.GetKey(crouchKey)) {
            movementState = MovementState.crouching;
            moveSpeed = crouchSpeed;
        } else if (grounded && Input.GetKey(sprintKey)) {
            movementState = MovementState.sprinting;
            moveSpeed = sprintSpeed;

        } else if (grounded) {
            movementState = MovementState.walking;
            moveSpeed = walkSpeed;

        } else {
            movementState = MovementState.air;
        }
    }
    private void MovePlayer() {

        if (activeGrapple) return;

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope) {
            player.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
            if (player.velocity.y > 0) {
                player.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        if (grounded)
            player.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        if (!grounded)
            player.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        if (!wallRunning) player.useGravity = !OnSlope();
    }

    private void SpeedControl() {

        if (activeGrapple) return;

        if (OnSlope() && !exitingSlope) {
            if (player.velocity.magnitude > moveSpeed) {
                player.velocity = player.velocity.normalized * moveSpeed;
            }
        } else {
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

    private bool OnSlope() {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f)) {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection() {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
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
