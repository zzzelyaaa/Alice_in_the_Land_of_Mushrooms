using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateRainbow : MonoBehaviour
{
    public Vector3 RotationAxcese = Vector3.right;
    public float RotationSpeed = 50;
    public float MinRange = 50;
    public float MaxRange = 150;
    public bool RotatClockWise = false ;
    private Transform MiTransform ;
    // Start is called before the first frame update
    void Start()
    {

        RotationSpeed = Random.Range(MinRange, MaxRange);
        RotatClockWise = Random.Range(0,100)>50;
        MiTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
       float direction = RotatClockWise  ? 1 : -1;
        MiTransform.Rotate(RotationAxcese, RotationSpeed * direction * Time.deltaTime);
    }
}
