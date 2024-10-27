using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NaughtyAttributes;

/// <summary>
/// Data container for each room that describes size, hiding spots, doorways, and patrol path.
/// Informs DoorwayArea's of what rooms they're connected to
/// </summary>
public class Room : MonoBehaviour
{
    [SerializeField]
    private string roomID;

    [SerializeField]
    private BoxCollider roomBox;

    public BoxCollider RoomBox => roomBox;

    public List<DoorwayArea> Doorways;

    public PatrolPath RoomPatrolPath;

    public List<HidingArea> HidingSpots = new();

    private void Start()
    {
        foreach (var d in Doorways)
            d.AddRoomConnection(this);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.black;
        GUI.color = Color.black;
        GUIStyle style = GUIStyle.none;
        style.fontSize = 40;
        style.alignment = TextAnchor.MiddleCenter;
        Handles.Label(transform.position + Vector3.up * 2f, roomID, style);
    }

    [Button("Scan doorways", EButtonEnableMode.Editor)]
    private void ScanDoorways()
    {
        if (TryGetComponent(out BoxCollider box))
        {
            var overlaps = Physics.OverlapBox(
                transform.position + box.center,
                box.size / 2f, Quaternion.identity,
                LayerMask.GetMask("Default"),
                QueryTriggerInteraction.Collide);

            if (overlaps.Length > 0)
                Doorways.Clear();

            foreach (var o in overlaps)
                if (o.CompareTag("Doorway"))
                    Doorways.Add(o.GetComponent<DoorwayArea>());
        }
    }
#endif
}
