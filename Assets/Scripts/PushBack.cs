using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushBack : MonoBehaviour
{
    private PlayerController PlayerControllerScript
    { get; set; }

    private EnemyBossAbilities EnemyBossAbilitiesScript
    { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        PlayerControllerScript = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (gameObject.CompareTag("FriendlyItem"))
        {
            PlayerControllerScript.PushBackEnemy(other);
        }

        if (gameObject.CompareTag("EnemyItem"))
        {
            // Should only be one boss object, with this script, within the Scene at any one time, so the below is acceptable for now.
            EnemyBossAbilitiesScript = FindObjectOfType<EnemyBossAbilities>();
            EnemyBossAbilitiesScript.PushBackPlayer(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {

    }
}
