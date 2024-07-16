using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    [field: SerializeField] public float SpeedOfRotation
    { get; set; }
    private float HorizontalControls
    {  get; set; }

    // Update is called once per frame
    void Update()
    {
        CameraRotationControls();
    }

    void CameraRotationControls()
    {
        HorizontalControls = Input.GetAxis("Horizontal");
        transform.Rotate(HorizontalControls * SpeedOfRotation * Time.deltaTime * Vector3.up);
    }
}
