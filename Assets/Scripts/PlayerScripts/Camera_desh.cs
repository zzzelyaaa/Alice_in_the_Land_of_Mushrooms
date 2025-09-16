using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_desh : MonoBehaviour
{
    private Transform Cameratransform;
    [SerializeField] private Vector3 CameraEndPosition=new Vector3(0,0,-10) ;
    [SerializeField] private float Cameraspeed = 5;
    private bool Complite=false;
    // Start is called before the first frame update
    void Start()
    {
      Cameratransform=transform; 
    }

    // Update is called once per frame
    void Update()
    {
        if (!Complite) { Cameratransform.Translate(Vector3.forward*Cameraspeed*Time.deltaTime);
            Complite = Vector3.Distance(Cameratransform.position , CameraEndPosition) < 1;
        }   
    }
}
