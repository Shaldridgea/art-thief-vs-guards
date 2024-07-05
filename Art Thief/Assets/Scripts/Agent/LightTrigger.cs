using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out VisualInterest interest))
            interest.IsLitUp = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out VisualInterest interest))
            interest.IsLitUp = false;
    }
}
