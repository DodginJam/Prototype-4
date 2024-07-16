using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDestroyEnemy : MonoBehaviour
{
    public Vector3 PlayerStartPosition
    { get; set; }
    private float ForceAppliedToEnemy = 100.0f;
    public float DespawnZone
    { get; set; } = 20.0f;
    public GameObject EnemyOfFocus
    { get; set; }
    private Rigidbody ProjectileRigidBody 
    { get; set; }
    public float ProjectileSpeed
    { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        PlayerStartPosition = GameObject.Find("Player").GetComponent<Transform>().position;

        ProjectileRigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // If the enemy gameobject the projectile is chasing get's destroyed, destroy the projectiles too.
        if (EnemyOfFocus == null)
        {
            Destroy(gameObject);
        }

        TrackEnemy();
        DestroyOnOutOfBounds();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // On projectiles contact with enemy gameObject, before destroying itself, push enemy gameObject away from contact point.
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Rigidbody enemyRigidBody = collision.gameObject.GetComponent<Rigidbody>();

            Vector3 awayFromProjectileDirection = (collision.gameObject.transform.position - transform.position).normalized;

            enemyRigidBody.AddForce(awayFromProjectileDirection * ForceAppliedToEnemy, ForceMode.Impulse);

            Destroy(gameObject);
        }
        if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }

    void TrackEnemy()
    {
        // Used to keep the object tracking the enemy, like a missle.
        if (EnemyOfFocus != null)
        {
            Vector3 directionToEnemy = (EnemyOfFocus.transform.position - transform.position).normalized;

            transform.Translate(directionToEnemy * Time.deltaTime * ProjectileSpeed);
        }
    }

    void DestroyOnOutOfBounds()
    {
        // Destroy any projectile outside a 20 metre squared volume around the player spawn position.
        if (transform.position.z > PlayerStartPosition.z + DespawnZone || transform.position.z < PlayerStartPosition.z + -DespawnZone)
        {
            Destroy(gameObject);
        }
        if (transform.position.x > PlayerStartPosition.x + DespawnZone || transform.position.x < PlayerStartPosition.x + -DespawnZone)
        {
            Destroy(gameObject);
        }
        if (transform.position.y > PlayerStartPosition.y + DespawnZone || transform.position.y < PlayerStartPosition.y + -DespawnZone)
        {
            Destroy(gameObject);
        }
    }
}
