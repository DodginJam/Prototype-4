using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PursuePlayer : MonoBehaviour
{
    private Rigidbody CurrentRigidbody
    { get; set; }
    public GameObject Player
    { get; set; }
    [field: SerializeField] public float Speed
    { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        CurrentRigidbody = GetComponent<Rigidbody>();

        Player = GameObject.Find("Player");
    }

    private void FixedUpdate()
    {
        Vector3 lookDirection = (Player.transform.position - transform.position).normalized;

        CurrentRigidbody.AddForce(lookDirection * Speed);
    }
}
