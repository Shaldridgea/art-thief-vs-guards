using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// Data container describing how a Utility action should be scored
/// </summary>
[CreateAssetMenu(fileName = "New Action", menuName = "Action Data")]
public class ActionData : ScriptableObject
{
    [SerializeField]
    private Consts.UtilityActionType actionType;

    /// <summary>
    /// The utility action this data will be used for
    /// </summary>
    public Consts.UtilityActionType Action => actionType;

    [SerializeField]
    private UtilityAction.ScoreCurve[] motiveCurves;

    /// <summary>
    /// The values used to score the utility of this action
    /// </summary>
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