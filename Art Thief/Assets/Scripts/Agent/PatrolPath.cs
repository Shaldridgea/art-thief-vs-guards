using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolPath : MonoBehaviour
{
    [SerializeField]
    private List<Transform> pointsList;

    public Vector3 GetPoint(int pointIndex) => pointsList[pointIndex % pointsList.Count].position;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        if(pointsList != null && pointsList.Count > 0)
        {
            for(int i = 0; i < pointsList.Count; ++i)
            {
                Gizmos.DrawLine(pointsList[i].position, pointsList[(i+1) % pointsList.Count].position);
            }
        }
    }
}
