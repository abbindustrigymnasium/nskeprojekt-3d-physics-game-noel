using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CustomBullet : MonoBehaviour
{
    //Assignables
    public Rigidbody rb;
    public GameObject explosion;
    public LayerMask whatIsEnemies;
    //Stats

    [Range(0f, 1f)]
    public float bounciness;
    public bool useGravity;
    //Damage

    public int explosionDamage;
    public float explosionRange;
    public float explosionForce;
    //Lifetime
    public int maxCollisions;
    public float maxLifetime;
    public bool explodeOnTouch = true;

    [SerializeField] private float _collisionMultiplier;

    int collisions;
    PhysicMaterial physics_mat;
    //public TextMeshProUGUI blocksDestroyed;


    private void Start() {
        Setup();
    }

    private void Update() {
        //When to explode:
        if (collisions > maxCollisions) Invoke("Delay", 0.02f);
        //Count down lifetime
        maxLifetime -= Time.deltaTime;
        if (maxLifetime <= 0) Invoke("Delay", 0.02f);

        //if (blocksDestroyed != null)
        //blocksDestroyed.SetText("Blocks Destroyed: " + collisions);
    }
    private void Explode() {

        //Instantiate explosion
        


        
    }

    private void Delay() {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision) {

        collisions++;
        if (explosion != null) Instantiate(explosion, transform.position, Quaternion.identity);
        print("Exploded");
        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange, whatIsEnemies);

        if ((collision.collider.CompareTag("Enemy") ||  collision.collider.CompareTag("Explosive") || collision.collider.CompareTag("Block")) && explodeOnTouch) {

            foreach (var obj in enemies) {
                var objRB = obj.GetComponent<Rigidbody>();
                if (objRB == null) continue;

                objRB.AddExplosionForce(collision.relativeVelocity.magnitude * _collisionMultiplier * explosionForce, collision.contacts[0].point, explosionRange);
            }
            
            Invoke("Delay", 0.02f);
        }
        else if (collision.collider.CompareTag("Player")) {
            foreach (var obj in enemies) {
                var objRB = obj.GetComponent<Rigidbody>();
                if (objRB == null) continue;

                objRB.AddExplosionForce(collision.relativeVelocity.magnitude * _collisionMultiplier * explosionForce * 10f, collision.contacts[0].point, explosionRange);
            }

            Invoke("Delay", 0.02f);
        }
    }

    
    private void Setup() {
        //Create a new Physic material
        physics_mat = new PhysicMaterial();
        physics_mat.bounciness = bounciness;
        physics_mat.frictionCombine = PhysicMaterialCombine.Minimum;
        physics_mat.bounceCombine = PhysicMaterialCombine.Maximum;

        //Assign material to collider
        GetComponent<SphereCollider>().material = physics_mat;
        rb.useGravity = useGravity;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}
