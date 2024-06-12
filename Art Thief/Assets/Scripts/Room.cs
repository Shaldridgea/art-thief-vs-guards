using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Room : MonoBehaviour
{
    [SerializeField]
    private string roomID;

    public List<DoorwayArea> Doorways;

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
}
