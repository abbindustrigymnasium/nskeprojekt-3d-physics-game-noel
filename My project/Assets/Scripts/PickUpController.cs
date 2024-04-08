using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpController : MonoBehaviour {
    public ProjectileGun gunScript;
    public Rigidbody rb;
    public BoxCollider coll;
    public Transform player, gunContainer, fpsCam;

    public float pickUpRange;
    public float dropForwardForce, dropUpwardForce;

    public bool equipped;
    public static bool slotFull;

    private void Start() {

        //Setup
        if (!equipped) {
            gunScript.enabled = false;
            rb.isKinematic = false;
            coll.isTrigger = false;
        }
        if (equipped) {
            gunScript.enabled = true;
            rb.isKinematic = true;
            coll.isTrigger = true;
            slotFull = true;
        }

    }

    private void Update() {
        //Check if player is in range and "E" is pressed
        Vector3 distanceToPlayer = player.position - transform.position;
        if (!equipped && distanceToPlayer.magnitude <= pickUpRange && Input.GetKeyDown(KeyCode.E) && !slotFull) PickUp();
        //Drop if equipped and "Q" is pressed
        if (equipped && Input.GetKeyDown(KeyCode.Q)) Drop();

        if (equipped && (transform.localPosition != Vector3.zero || transform.localRotation != Quaternion.Euler(Vector3.zero) || transform.localScale != Vector3.one)) {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(Vector3.zero);
            // transform.localRotation = player.transform.rotation;
            transform.localScale = Vector3.one;
        }
            
    }

    private void PickUp() {

        equipped = true;
        slotFull = true;

        rb.isKinematic = true;
        coll.isTrigger = true;

        //Make weapon a child of the camera and move it to default position
        transform.SetParent(gunContainer);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        // transform.localRotation = player.transform.rotation;
        transform.localScale = Vector3.one;


        gunScript.enabled = true;
    }

    private void Drop() {

        equipped = false;
        slotFull = false;

        transform.SetParent(null);


        rb.isKinematic = false;
        coll.isTrigger = false;

        rb.velocity = player.GetComponent<Rigidbody>().velocity;
        rb.AddForce(fpsCam.forward * dropForwardForce, ForceMode.Impulse);
        rb.AddForce(fpsCam.up * dropUpwardForce, ForceMode.Impulse);

        float randomRotation = Random.Range(-1f, 1f);
        rb.AddTorque(new Vector3(randomRotation, randomRotation, randomRotation) * 10);

        gunScript.enabled = false;
    }
}