using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnFall : MonoBehaviour
{
    private float FallHeightLimit
    { get; set; } = -10.0f;

    private SpawnManager SpawnManagerScript
    { get; set; }
    private EnemyBossAbilities EnemyBossAbilitiesScript
    { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        SpawnManagerScript = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
    }

    // Update is called once per frame
    void Update()
    {
        DestroyOnHeightLimit();
    }

    void DestroyOnHeightLimit()
    {
        bool destroyGameObject = false;

        if (transform.position.y <= FallHeightLimit)
        {
            if (!gameObject.name.Contains("EnemyBoss"))
            {
                destroyGameObject = true;
            }
            else
            {
                EnemyBossAbilitiesScript = gameObject.GetComponent<EnemyBossAbilities>();

                if (EnemyBossAbilitiesScript.CanBeDestroyed == false)
                {
                    EnemyBossAbilitiesScript.ComeBackOnFall();
                }
                else
                {
                    destroyGameObject = true;
                }
            }
        }

        if (destroyGameObject)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Remove the too be destroyed enemy gameObject from the list of active gameObjects in the scene.
        SpawnManagerScript.EnemiesInScene.Remove(gameObject);
    }
}
