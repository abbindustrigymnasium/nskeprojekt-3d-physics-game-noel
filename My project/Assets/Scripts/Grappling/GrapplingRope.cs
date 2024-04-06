using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingRope : MonoBehaviour
{
    Grappling grapple;
    private void LateUpdate() {
        DrawRope();
    }

    private void Start() {
        grapple = GetComponent<Grappling>();

    }

    void DrawRope() {

        grapple.lr.SetPosition(0, grapple.gunTip.position);
        grapple.lr.SetPosition(1, grapple.grapplePoint);

    }
}
