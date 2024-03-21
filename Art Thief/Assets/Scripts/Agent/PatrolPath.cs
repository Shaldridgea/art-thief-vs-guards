using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolPath : MonoBehaviour
{
    [SerializeField]
    private List<Transform> pointsList;

    public Vector3 GetPoint(int pointIndex) => pointsList[pointIndex % pointsList.Count].position;
}
