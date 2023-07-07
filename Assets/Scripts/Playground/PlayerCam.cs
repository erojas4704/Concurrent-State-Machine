using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public Transform target;
    void Start()
    {
        
    }

    void LateUpdate()
    {
        transform.position = target.position + new Vector3(0, 10f, -5f);
        transform.LookAt(target);
    }
}
