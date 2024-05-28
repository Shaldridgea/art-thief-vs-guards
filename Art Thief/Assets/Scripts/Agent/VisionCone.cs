using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionCone : MonoBehaviour
{
    [SerializeField]
    private bool isCentralVision;

    public bool IsCentralVision => isCentralVision;

    public delegate void VisionDelegate(VisionCone callingCone, GameObject spottedVisual);

    public event VisionDelegate TriggerEnter;

    public event VisionDelegate TriggerExit;

    private void OnTriggerEnter(Collider other)
    {
        TriggerEnter?.Invoke(this, other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        TriggerExit?.Invoke(this, other.gameObject);
    }
}