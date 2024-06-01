using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HidingArea : MonoBehaviour
{
    [SerializeField]
    private Consts.HidingAreaType areaType;

    [SerializeField]
    private BoxCollider boxArea;

    public bool IsSafe { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        IsSafe = areaType == Consts.HidingAreaType.Safe;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (boxArea == null)
            return;
        
        Color gizColor = default;
        switch (areaType)
        {
            case Consts.HidingAreaType.Safe:
                gizColor = Color.green;
                break;

            case Consts.HidingAreaType.Dynamic:
                gizColor = Color.blue;
                break;

            case Consts.HidingAreaType.Conditional:
                gizColor = Color.red;
                break;
        }
        gizColor.a = 0.3f;
        Gizmos.color = gizColor;
        Gizmos.DrawCube(transform.position + boxArea.center, boxArea.size);
    }

    private void OnValidate()
    {
        if (boxArea == null)
            if (TryGetComponent(out BoxCollider box))
                boxArea = box;
    }
}
