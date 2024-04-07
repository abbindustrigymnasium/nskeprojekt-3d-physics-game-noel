using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour {
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private CharacterMovement cm;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    void Start() {
        rb = GetComponent<Rigidbody>();
        cm = GetComponent<CharacterMovement>();

        startYScale = playerObj.localScale.y;
    }

    private void Update() {

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
            StartSlide();

        if (Input.GetKeyUp(slideKey) && cm.sliding)
            StopSlide();
    }
    private void FixedUpdate() {
        if (cm.sliding) {
            SlidingMovement();
        }
    }

    private void StartSlide() {
        cm.sliding = true;
        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        slideTimer = maxSlideTime;
    }

    private void SlidingMovement() {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(!cm.OnSlope() || rb.velocity.y > -0.1f) {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
            slideTimer -= Time.deltaTime;
        }
        else {
            rb.AddForce(cm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);

        }

        slideTimer -= Time.deltaTime;

        if (slideTimer <= 0) {
            StopSlide();
        }
    }

    private void StopSlide() {
        cm.sliding = false;
        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
}