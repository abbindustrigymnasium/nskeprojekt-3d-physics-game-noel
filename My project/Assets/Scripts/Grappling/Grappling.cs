using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour {
    [Header("References")]
    private CharacterMovement cm;
    public Transform camComponent;

    public Transform gunTip;
    public LayerMask whatIsGrappleable;
    public LineRenderer lr;

    [Header("Grappling")]
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overshootYAxis;

    public Vector3 grapplePoint;

    [Header("Cooldown")]
    public float grapplingCd;
    private float grapplingCdTimer;

    [Header("Input")]
    public KeyCode grappleKey = KeyCode.Mouse1;

    private bool grappling;

    private void Start() {
        cm = GetComponent<CharacterMovement>();
    }

    //private void Awake() {
        //lr = GetComponent<LineRenderer>();
    //}

    private void Update() {
        if (Input.GetKeyDown(grappleKey)) StartGrapple();

        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;
    }

    private void LateUpdate() {
        if (grappling)
            lr.SetPosition(0, gunTip.position);
    }

    private void StartGrapple() {
        if (grapplingCdTimer > 0) return;
        grappling = true;

        RaycastHit hit;
        if (Physics.Raycast(camComponent.position, camComponent.forward, out hit, maxGrappleDistance, whatIsGrappleable)) {
            cm.freeze = true;
            grapplePoint = hit.point;
            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else {
            grapplePoint = camComponent.position + camComponent.forward * maxGrappleDistance;
            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        lr.enabled = true;
        Debug.Log("Line Renderer");
        lr.SetPosition(1, grapplePoint);
    }

    private void ExecuteGrapple() {
        cm.freeze = false;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

        cm.JumpToPosition(grapplePoint, highestPointOnArc);
        Invoke(nameof(StopGrapple), 1f);

    }

    public void StopGrapple() {
        grappling = false;
        grapplingCdTimer = grapplingCd;

        lr.enabled = false;
    }
}
