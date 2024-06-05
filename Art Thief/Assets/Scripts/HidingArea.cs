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

    // Start is called before the first frame update
    void Start()
    {
        IsSafe = areaType == Consts.HidingAreaType.Safe;
    }

    public void CheckForSafety(List<GuardAgent> threats)
    {
        if(areaType == Consts.HidingAreaType.Safe)
            return;

        for(int i = 0; i < threats.Count; ++i)
        {
            GuardAgent guard = threats[i];
            float angleToArea = Vector3.Angle(guard.transform.forward, (transform.position - guard.transform.position).normalized);
            if (angleToArea <= 50f)
            {
                if (!Physics.Linecast(guard.transform.position, transform.position, guard.Senses.LosMask, QueryTriggerInteraction.Ignore))
                {
                    IsSafe = false;
                    return;
                }
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
