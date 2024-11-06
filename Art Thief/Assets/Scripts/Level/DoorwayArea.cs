using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Describes the area between two rooms and the points where the doorway connects to the rooms
/// </summary>
public class DoorwayArea : MonoBehaviour
{
    [SerializeField]
    private Transform[] endPoints;

    public List<Vector3> EndPoints { get; private set; } = new();

    [SerializeField]
    private BoxCollider boxArea;

    [SerializeField]
    [Tooltip("How risky is this doorway to use for the Thief e.g. doorways leading to a dead end are high risk")]
    private float baseRisk;

    public float Risk { get; private set; }

    public List<Room> ConnectedRooms { get; private set; } = new();

    const float THIEF_MIN_RISK_DISTANCE = 5f;

    const float THIEF_MAX_RISK_DISTANCE = 25f;

    const float GUARD_MIN_RISK_DISTANCE = 7f;

    const float GUARD_MAX_RISK_DISTANCE = 18f;

    void Start()
    {
        UpdateEndPoints();
    }

    /// <summary>
    /// Calculate how risky this doorway is for the thief to currently travel to it
    /// </summary>
    public void CalculateRisk(ThiefAgent thief, List<GuardAgent> threats)
    {
        Risk = 0f;
        // Add risk based on how far away the thief is from the start of this doorway
        // Further away doorways are higher risk
        Risk += Mathf.InverseLerp(THIEF_MIN_RISK_DISTANCE, THIEF_MAX_RISK_DISTANCE,
            Vector3.Distance(GetNearestPoint(thief.transform.position), thief.transform.position)) * 1.5f;

        // Risk factors contributed by guards
        int threatCount = 0;
        for (int i = 0; i < threats.Count; ++i)
        {
            GuardAgent guard = threats[i];

            // Stunned guards aren't a threat to us
            if (guard.AgentBlackboard.GetVariable<bool>(Consts.AGENT_STUN_STATUS))
                continue;

            ++threatCount;
            Vector3 nearPoint = GetNearestPoint(guard.transform.position);
            // Add risk to this doorway the closer a guard is to it
            Risk += Mathf.InverseLerp(GUARD_MAX_RISK_DISTANCE, GUARD_MIN_RISK_DISTANCE,
                Vector3.Distance(nearPoint, guard.transform.position));

            // Increase our risk significantly if a guard can see this doorway
            if (guard.Senses.IsSeen(nearPoint))
                Risk += 0.7f;

            float guardToDoorwayAngle = Vector3.Angle(
            (guard.transform.position - thief.transform.position).normalized,
            (nearPoint - thief.transform.position).normalized);

            // Increase risk if the thief going to this doorway would
            // mean they are going in the direction of a guard
            if (guardToDoorwayAngle <= 35f)
                Risk += 0.4f;
        }

        // Average our risk from multiple guards
        Risk /= Mathf.Max(threatCount, 1);

        Risk += baseRisk;
    }

    /// <summary>
    /// Get the nearest entrance point of this doorway from the supplied position
    /// </summary>
    public Vector3 GetNearestPoint(Vector3 position)
    {
        if (EndPoints.Count != 2)
            return position;

        float firstDistance = Vector3.Distance(position, EndPoints[0]);
        float secondDistance = Vector3.Distance(position, EndPoints[1]);
        return firstDistance < secondDistance ? EndPoints[0] : EndPoints[1];
    }

    /// <summary>
    /// Get the furthest entrance point of this doorway from the supplied position
    /// </summary>
    public Vector3 GetFurthestPoint(Vector3 position)
    {
        if (EndPoints.Count != 2)
            return position;

        float firstDistance = Vector3.Distance(position, EndPoints[0]);
        float secondDistance = Vector3.Distance(position, EndPoints[1]);
        return firstDistance > secondDistance ? EndPoints[0] : EndPoints[1];
    }

    public void AddRoomConnection(Room addRoom) => ConnectedRooms.Add(addRoom);

    /// <summary>
    /// Cache the points of the GameObjects marking the ends of this doorway
    /// </summary>
    private void UpdateEndPoints()
    {
        if (endPoints == null || endPoints.Length == 0)
            return;

        EndPoints.Clear();
        foreach (var t in endPoints)
            EndPoints.Add(t.position);
    }

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
   
    private void OnValidate()
    {
        if (boxArea == null)
            if (TryGetComponent(out BoxCollider box))
                boxArea = box;

        UpdateEndPoints();
    }
}
