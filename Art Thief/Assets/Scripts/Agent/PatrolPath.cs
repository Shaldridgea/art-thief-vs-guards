using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolPath : MonoBehaviour
{
    [SerializeField]
    private Color pathColour = Color.blue;

    [SerializeField]
    private List<Transform> waypointList;

    private List<Vector3> pointsList = new List<Vector3>(10);

    public Vector3 GetPoint(int pointIndex) => pointsList[pointIndex % pointsList.Count];

    private void Start()
    {
        UpdatePoints();
    }

    public Vector3 GetNextPointFromPosition(Vector3 startPosition)
    {
        float compareDistance = float.MaxValue;
        int compareIndex = -1;
        for(int i = 0; i < pointsList.Count; ++i)
        {
            float checkDistance = Vector3.Distance(startPosition, pointsList[i]);
            if (checkDistance < compareDistance)
            {
                compareDistance = checkDistance;
                compareIndex = i;
            }
        }
        if(compareDistance <= 5f)
        {
            ++compareIndex;
            if (compareIndex > pointsList.Count)
                compareIndex = 0;
        }
        return pointsList[compareIndex];
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = pathColour;
        if(pointsList != null && pointsList.Count > 0)
        {
            for(int i = 0; i < pointsList.Count; ++i)
            {
                Vector3 start = pointsList[i];
                Vector3 end = pointsList[(i + 1) % pointsList.Count];
                Gizmos.DrawLine(start, end);
                Gizmos.DrawWireSphere(start, 5f);
            }
        }
    }

    private void OnValidate()
    {
        UpdatePoints();
    }

    private void UpdatePoints(){
        if (pointsList == null)
            pointsList = new List<Vector3>(10);

        pointsList.Clear();

        for (int i = 0; i < waypointList.Count; ++i)
        {
            if (waypointList[i] != null)
                pointsList.Add(waypointList[i].position);
        }
    }
}
