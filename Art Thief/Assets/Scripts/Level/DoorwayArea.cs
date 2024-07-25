using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DoorwayArea : MonoBehaviour
{
    [SerializeField]
    private Transform[] endPoints;

    public List<Vector3> EndPoints { get; private set; } = new();

    [SerializeField]
    private BoxCollider boxArea;

    [SerializeField]
    private float baseRisk;

    public float Risk { get; private set; }

    public List<Room> ConnectedRooms { get; private set; } = new();

    // Start is called before the first frame update
    void Start()
    {
        UpdateEndPoints();
    }

    public void CalculateRisk(ThiefAgent thief, List<GuardAgent> threats)
    {
        Risk = 0f;
        Risk += Mathf.InverseLerp(5f, 25f,
            Vector3.Distance(GetNearestPoint(thief.transform.position), thief.transform.position)) * 1.5f;
        int threatCount = 0;
        for (int i = 0; i < threats.Count; ++i)
        {
            GuardAgent guard = threats[i];

            if (guard.AgentBlackboard.GetVariable<bool>("isStunned"))
                continue;

            ++threatCount;
            Vector3 nearPoint = GetNearestPoint(guard.transform.position);
            Risk += Mathf.InverseLerp(18f, 7f, Vector3.Distance(nearPoint, guard.transform.position));
            float angleToDoorway = Vector3.Angle(guard.transform.forward, (nearPoint - guard.transform.position).normalized);

            if (angleToDoorway <= 50f)
                Risk += 0.7f;

            float angleGuardToDoorway = Vector3.Angle(
            (guard.transform.position - thief.transform.position).normalized,
            (nearPoint - thief.transform.position).normalized);

            if (angleGuardToDoorway <= 35f)
                Risk += 0.4f;
        }
        Risk /= Mathf.Max(threatCount, 1);
        Risk += baseRisk;
    }

    public Vector3 GetNearestPoint(Vector3 position)
    {
        if (EndPoints.Count != 2)
            return position;

        float firstDistance = Vector3.Distance(position, EndPoints[0]);
        float secondDistance = Vector3.Distance(position, EndPoints[1]);
        return firstDistance < secondDistance ? EndPoints[0] : EndPoints[1];
    }

    public Vector3 GetFurthestPoint(Vector3 position)
    {
        if (EndPoints.Count != 2)
            return position;

        float firstDistance = Vector3.Distance(position, EndPoints[0]);
        float secondDistance = Vector3.Distance(position, EndPoints[1]);
        return firstDistance > secondDistance ? EndPoints[0] : EndPoints[1];
    }

    public void AddRoomConnection(Room addRoom) => ConnectedRooms.Add(addRoom);

    private void OnDrawGizmos()
    {
        if (boxArea == null)
            return;

        Color gizColor = Color.blue;
        gizColor.a = 0.3f;
        Gizmos.color = gizColor;
        Gizmos.DrawCube(transform.position + boxArea.center, boxArea.size);
        foreach (Transform t in endPoints)
            Gizmos.DrawSphere(t.position, 0.25f);
    }

    private void UpdateEndPoints()
    {
        if (endPoints == null || endPoints.Length == 0)
            return;

        EndPoints.Clear();
        foreach (var t in endPoints)
            EndPoints.Add(t.position);
    }
   
    private void OnValidate()
    {
        if (boxArea == null)
            if (TryGetComponent(out BoxCollider box))
                boxArea = box;

        UpdateEndPoints();
    }
}
