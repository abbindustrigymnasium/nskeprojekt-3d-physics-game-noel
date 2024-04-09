using UnityEngine;

public class ItemSwitch : MonoBehaviour {
    public GameObject grappler; // Assign your first item in the Unity editor
    public GameObject swinger; // Assign your second item in the Unity editor
    public GameObject gunItem; // Assign your third item in the Unity editor

    public bool grapplingAndSwiningAvailable;

    private void Awake() {
        grapplingAndSwiningAvailable = true;

        grappler.SetActive(true);
        swinger.SetActive(true);
        gunItem.SetActive(false);
    }
    public void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {

            grapplingAndSwiningAvailable = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) {

            grapplingAndSwiningAvailable = false;        
        }

        if (grapplingAndSwiningAvailable) {
            // Enable item1 and disable item2 when the "1" key is pressed
            grappler.SetActive(true);
            swinger.SetActive(true);
            gunItem.SetActive(false);
        }
        else {
            // Enable item2 and disable item1 when the "2" key is pressed
            grappler.SetActive(false);
            swinger.SetActive(false);
            gunItem.SetActive(true);
        }
    }
    public bool IsGrappling() {
        return grapplingAndSwiningAvailable;
    }
}
