using UnityEngine;
public class Breakable : MonoBehaviour {
    [SerializeField] private GameObject _replacement;
    [SerializeField] private float _breakForce = 10;
    [SerializeField] private float _collisionMultiplier = 100;
    [SerializeField] private bool _broken;

    public ProjectileGun projectileGun;

    void OnCollisionEnter(Collision collision) {
        if (_broken) return;
        if (collision.relativeVelocity.magnitude >= _breakForce) {
            print(collision.relativeVelocity.magnitude);
            _broken = true;
            var replacement = Instantiate(_replacement, transform.position, transform.rotation);

            var rbs = replacement.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in rbs) {
                rb.AddExplosionForce(collision.relativeVelocity.magnitude * _collisionMultiplier, collision.contacts[0].point, 2);
                // projectileGun.destroyedBlocks++;
            }

            Destroy(gameObject);

            Destroy(replacement, 5f);
        }
        else return;
    }
}