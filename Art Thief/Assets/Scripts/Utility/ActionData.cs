using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "New Action", menuName = "Action Data")]
public class ActionData : ScriptableObject
{
    [SerializeField]
    private Consts.UtilityActionType actionType;

    public Consts.UtilityActionType Action => actionType;

    [SerializeField]
    private UtilityAction.ScoreCurve[] motiveCurves;

    public UtilityAction.ScoreCurve[] Motives => motiveCurves;

    private void OnValidate()
    {
        if (motiveCurves == null)
            return;

        if (motiveCurves.Length == 0)
            return;

        for (int i = 0; i < motiveCurves.Length; ++i)
            if (motiveCurves[i].Curve != null)
            {
                motiveCurves[i].Curve.preWrapMode = WrapMode.ClampForever;
                motiveCurves[i].Curve.postWrapMode = WrapMode.ClampForever;
            }
    }
}