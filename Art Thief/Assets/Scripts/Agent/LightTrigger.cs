using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightTrigger : MonoBehaviour
{
    [SerializeField]
    private Light lightSource;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out VisualInterest interest))
            interest.EnteredLight(lightSource);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out VisualInterest interest))
            interest.ExitedLight(lightSource);
    }
}
