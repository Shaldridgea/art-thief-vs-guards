using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolPath : MonoBehaviour
{
    [SerializeField]
    private Color pathColour = Color.blue;

    [SerializeField]
    private List<Transform> waypointList = new();

    private List<Vector3> pointsList = new List<Vector3>(10);

    public Vector3 GetPoint(int pointIndex) => pointsList[pointIndex % pointsList.Count];

    private void Start()
    {
        UpdatePoints();
    }

    public Vector3 FindNextPointFromPosition(Vector3 startPosition)
    {
        float compareDistance = float.MaxValue;
        int compareIndex = -1;
        startPosition = startPosition.ZeroY();
        for(int i = 0; i < pointsList.Count; ++i)
        {
            float checkDistance = Vector3.Distance(startPosition, pointsList[i].ZeroY());
            if (checkDistance < compareDistance)
            {
                compareDistance = checkDistance;
                compareIndex = i;
            }
        }

        Vector3 nextPoint = pointsList[(compareIndex + 1) % pointsList.Count].ZeroY();
        Vector3 comparePoint = pointsList[compareIndex].ZeroY();
        Vector3 pathAngle = (nextPoint - comparePoint).normalized;
        Vector3 startAngle = (comparePoint - startPosition).normalized;

        if (Vector3.Distance(startPosition, comparePoint) < 2f || Vector3.Dot(pathAngle, startAngle) < 0.3f)
            compareIndex = (compareIndex + 1) % pointsList.Count;

        return pointsList[compareIndex];
    }

    public Vector3 GetRandomPosition(Vector3 startPosition)
    {
        float compareDistance = float.MaxValue;
        int compareIndex = -1;
        startPosition = startPosition.ZeroY();
        for (int i = 0; i < pointsList.Count; ++i)
        {
            float checkDistance = Vector3.Distance(startPosition, pointsList[i].ZeroY());
            if (checkDistance < compareDistance)
            {
                compareDistance = checkDistance;
                compareIndex = i;
            }
        }

        List<Vector3> excludedPointsList = new(pointsList);
        excludedPointsList.RemoveAt(compareIndex);

        return excludedPointsList[Random.Range(0, excludedPointsList.Count)];
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = pathColour;
        UpdatePoints();
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

    private void UpdatePoints()
    {
        pointsList.Clear();

        for (int i = 0; i < waypointList.Count; ++i)
        {
            if (waypointList[i] != null)
                pointsList.Add(waypointList[i].position);
        }
    }

    private void Reset()
    {
        waypointList.Clear();
        for(int i = 0; i < transform.childCount; ++i)
        {
            waypointList.Add(transform.GetChild(i));
        }
        UpdatePoints();
    }
}
