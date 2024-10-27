using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Container for a list of points describing a patrol path
/// </summary>
public class PatrolPath : MonoBehaviour
{
    [SerializeField]
    private Color pathColour = Color.blue;

    [SerializeField]
    private List<Transform> waypointList = new();

    private List<Vector3> pointsList = new List<Vector3>(10);

    private void Start()
    {
        UpdatePoints();
    }

    public Vector3 GetPoint(int pointIndex) => pointsList[pointIndex % pointsList.Count];

    /// <summary>
    /// Find the next logical point to follow on this path from the starting position.
    /// This takes into account distance to a point and the angle you'd take from the
    /// start position to try and make following the path look more natural
    /// </summary>
    public Vector3 FindNextPointFromPosition(Vector3 startPosition)
    {
        float compareDistance = float.MaxValue;
        int compareIndex = -1;
        startPosition = startPosition.ZeroY();
        // Find the closest point
        for(int i = 0; i < pointsList.Count; ++i)
        {
            float checkDistance = Vector3.Distance(startPosition, pointsList[i].ZeroY());
            if (checkDistance < compareDistance)
            {
                compareDistance = checkDistance;
                compareIndex = i;
            }
        }

        // Get our closest point and the next point from it along the path,
        // as well as the angles to those
        Vector3 secondPoint = pointsList[(compareIndex + 1) % pointsList.Count].ZeroY();
        Vector3 firstPoint = pointsList[compareIndex].ZeroY();
        Vector3 pathAngle = (secondPoint - firstPoint).normalized;
        Vector3 startAngle = (firstPoint - startPosition).normalized;

        // If we're already basically on top of the first point, or if
        // going to the first point would look weird because we'd be going in the,
        // opposite direction of our path go to our second point instead
        if (Vector3.Distance(startPosition, firstPoint) < 2f || Vector3.Dot(pathAngle, startAngle) < 0.3f)
            compareIndex = (compareIndex + 1) % pointsList.Count;

        return pointsList[compareIndex];
    }

    /// <summary>
    /// Get a random point on the path, excluding the closest point to the starting position
    /// </summary>
    public Vector3 GetRandomPosition(Vector3 startPosition)
    {
        float compareDistance = float.MaxValue;
        int compareIndex = -1;
        startPosition = startPosition.ZeroY();
        // Find our closest position to start
        for (int i = 0; i < pointsList.Count; ++i)
        {
            float checkDistance = Vector3.Distance(startPosition, pointsList[i].ZeroY());
            if (checkDistance < compareDistance)
            {
                compareDistance = checkDistance;
                compareIndex = i;
            }
        }

        // Get a random point with the closest one excluded
        List<Vector3> excludedPointsList = new(pointsList);
        excludedPointsList.RemoveAt(compareIndex);

        return excludedPointsList[Random.Range(0, excludedPointsList.Count)];
    }

    /// <summary>
    /// Cache the points of the GameObjects that make up this path
    /// </summary>
    private void UpdatePoints()
    {
        pointsList.Clear();

        for (int i = 0; i < waypointList.Count; ++i)
        {
            if (waypointList[i] != null)
                pointsList.Add(waypointList[i].position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = pathColour;
        UpdatePoints();
        if (pointsList != null && pointsList.Count > 0)
        {
            for (int i = 0; i < pointsList.Count; ++i)
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
