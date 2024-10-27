using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HidingArea : MonoBehaviour
{
    [SerializeField]
    private Consts.HidingAreaType areaType;

    public Consts.HidingAreaType AreaType => areaType;

    [SerializeField]
    private BoxCollider boxArea;

    public bool IsSafe { get; private set; }

    void Start()
    {
        IsSafe = areaType == Consts.HidingAreaType.Safe;
    }

    public void CheckForSafety(List<GuardAgent> threats)
    {
        if(areaType == Consts.HidingAreaType.Safe)
            return;

        // Mark ourselves as unsafe if we're seen
        // by any guard we're aware of
        for (int i = 0; i < threats.Count; ++i)
        {
            GuardAgent guard = threats[i];
            if (guard.GuardSenses.IsInLOS(transform.position))
            {
                IsSafe = false;
                return;
            }
        }
        IsSafe = true;
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
