using UnityEngine;
using TMPro;

public class ProjectileGun : MonoBehaviour
{
    // BULLET

    public GameObject bullet;
    public float shootForce, upwardForce;

    // GUN STATS
    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots;
    public int magSize, bulletsPerTap;
    public bool allowButtonHold;

    int bulletsLeft, bulletsShot;

    // BOOLS
    bool shooting, readyToShoot, reloading;

    // REFERENCES
    public Camera fpsCam;
    public Transform attackPoint;

    // GRAPHICS
    public GameObject muzzleFlash;
    public TextMeshProUGUI ammunitionDisplay;

    [Header("Buttons")]
    public KeyCode shootButton;
    public KeyCode aimbutton;
    public KeyCode reloadButton;

    public bool allowInvoke;

    private void Awake() {
        bulletsLeft = magSize;
        readyToShoot = true;
    }

    private void Update() {
        MyInput();

        if (ammunitionDisplay != null)
            ammunitionDisplay.SetText(bulletsLeft / bulletsPerTap + "/" + magSize / bulletsPerTap);
    }

    private void MyInput() {
        // Tap fire or automatic
        if (allowButtonHold) shooting = Input.GetKey(shootButton);
        else shooting = Input.GetKeyDown(shootButton);

        // reload normal
        if (Input.GetKeyDown(reloadButton) && bulletsLeft < magSize && !reloading) 
            Reload();

        // auto reload if shooting w empty mag
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0)
            Reload();

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0) {

            bulletsShot = 0;

            Shoot();
        }
    }

    private void Shoot() {
        readyToShoot = false;

        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(75);

        Vector3 noSpreadDirection = targetPoint - attackPoint.position;

        // SPREAD
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        Vector3 spreadDirection = noSpreadDirection + new Vector3(x, y, 0);

        // BULLET INSTANTIATION
        GameObject currentBullet = Instantiate(bullet, attackPoint.position, Quaternion.identity);

        currentBullet.transform.forward = spreadDirection.normalized;

        currentBullet.GetComponent<Rigidbody>().AddForce(spreadDirection.normalized * shootForce, ForceMode.Impulse);
        currentBullet.GetComponent<Rigidbody>().AddForce(fpsCam.transform.up * upwardForce, ForceMode.Impulse);

        if(muzzleFlash  != null) {
            Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);
        }

        bulletsLeft--;
        bulletsShot++;

        if (allowInvoke) {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;
        }

        if (bulletsShot < bulletsPerTap && bulletsLeft > 0) {
            Invoke("Shoot", timeBetweenShots);
        }
    }

    private void ResetShot() {
        readyToShoot = true;
        allowInvoke = true;
    }

    private void Reload() {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished() {
        bulletsLeft = magSize;
        reloading = false;
    }
}
