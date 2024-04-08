using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;
    public Transform camHolder;

    float xRotation;
    float yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // PLAYER PANNING CONTROLLER
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX; // mouseX roterar runt en axel som ligger horizontelt med spelarens synf�lt.
        xRotation -= mouseY; // mouseY roterar runt en axel som g�r igenom spelaren uppifr�n och ner.
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // begr�nsa r�ckvidden f�r att kolla upp och ner s� att spelarens synf�lt inte kan v�ndas upp och ner.

        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void DoFov(float endValue) {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }
    public void DoTilt(float zTilt) {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }
}
