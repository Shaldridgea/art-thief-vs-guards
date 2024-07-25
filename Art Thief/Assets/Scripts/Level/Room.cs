using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NaughtyAttributes;

public class Room : MonoBehaviour
{
    [SerializeField]
    private string roomID;

    public List<DoorwayArea> Doorways;

    public List<HidingArea> HidingSpots = new();

    private void Start()
    {
        foreach (var d in Doorways)
            d.AddRoomConnection(this);
    }

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
}
