using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardObject : MonoBehaviour
{
    private Camera facingCamera;

    private void Start()
    {
        facingCamera = Camera.main;
    }

    private void LateUpdate()
    {
        transform.LookAt(facingCamera.transform);
    }
}
