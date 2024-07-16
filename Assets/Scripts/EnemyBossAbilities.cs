using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBossAbilities : MonoBehaviour
{
    [field: SerializeField] public GameObject EnemyGuardPrefab
    { get; set; }
    private bool ReadyToSpawnGuard
    { get; set; } = true;
    private bool HasPushBack
    { get; set; } = true;
    [field: SerializeField] public GameObject PushBackObject
    { get; set; }
    public Vector3 PushBackObjectOffset
    { get; set; } = new Vector3(0, -0.35f, 0);
    private float PushBackForceMultiplier
    { get; set; } = 250.0f;
    bool FlashLightCoruntinesRunning
    { get; set; } = false;
    public int EnemyBossShieldHealth 
    { get; set; } = 3;
    public bool CanBeDestroyed
    { get; set; } = false;

    private SpawnManager SpawnManagerScript
    { get; set; }
    private PlayerController PlayerControllerScript
    { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        PushBackObject = Instantiate(PushBackObject, transform.position + PushBackObjectOffset, Quaternion.identity);

        SpawnManagerScript = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();

        PlayerControllerScript = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        PushBackObject.transform.position = transform.position + PushBackObjectOffset;

        // Once the shield is done, the boss can be destroyed.
        if (EnemyBossShieldHealth == 0 && !CanBeDestroyed)
        {
            PushBackObject.SetActive(false);
            CanBeDestroyed = true;
        }

        SpawnGuard();
    }

    public void ComeBackOnFall()
    {
        EnemyBossShieldHealth--;
        transform.position = SpawnManagerScript.GenerateSpawnPosition(true);
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().rotation = Quaternion.Euler(0, 0, 0);
        
        if (EnemyBossShieldHealth < 3 && EnemyBossShieldHealth > 0)
        {
            SpawnManagerScript.SpawnRandomPrefab();
        }
    }

    public void SpawnGuard()
    {
        if (EnemyBossShieldHealth <= 2 && ReadyToSpawnGuard && !PlayerControllerScript.GameOver)
        {
            GameObject newEnemy = Instantiate(EnemyGuardPrefab, SpawnManagerScript.GenerateSpawnPosition(true), Quaternion.identity);
            SpawnManagerScript.EnemiesInScene.Add(newEnemy);

            ReadyToSpawnGuard = false;
            StartCoroutine(SpawnGuardCooldown());
        }
    }

    IEnumerator SpawnGuardCooldown()
    {
        yield return new WaitForSeconds(4.0f);
        ReadyToSpawnGuard = true;
    }

    // Called in the OnTrigger of the PushBack script, as these are attached to the PushBackObject.
    public void PushBackPlayer(Collider other)
    {
        // Provide a forceful push on enemies when PushBack power up is active.
        if (other.gameObject.CompareTag("Player") && HasPushBack)
        {
            Rigidbody enemyRigidBody = other.gameObject.GetComponent<Rigidbody>();
            Vector3 awayFromPlayerDirection = (other.gameObject.transform.position - transform.position).normalized;

            enemyRigidBody.AddForce(awayFromPlayerDirection * PushBackForceMultiplier, ForceMode.Impulse);

            if (FlashLightCoruntinesRunning)
            {
                StopCoroutine(FlashOnPushBackTrigger());
            }
            StartCoroutine(FlashOnPushBackTrigger());
        }
    }

    IEnumerator FlashOnPushBackTrigger()
    {
        float defaultIntensity = 2.0f;
        float onTriggerIntensity = 5.0f;

        Light light = PushBackObject.GetComponentInChildren<Light>();

        // Light intensity quickly increases on contact, before returning to normal at a slower rate.
        for (float currentLightIntensity = light.intensity; currentLightIntensity < onTriggerIntensity; currentLightIntensity += 0.25f)
        {
            light.intensity = currentLightIntensity;
            yield return new WaitForSeconds(0.005f);
        }

        for (float currentLightIntensity = light.intensity; currentLightIntensity > defaultIntensity; currentLightIntensity -= 0.25f)
        {
            light.intensity = currentLightIntensity;
            yield return new WaitForSeconds(0.05f);
        }
    }

    private void OnDestroy()
    {
        Destroy(PushBackObject);
    }
}
