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
    private UtilityAction.MotiveUtility[] actionMotives;

    public UtilityAction.MotiveUtility[] Motives => actionMotives;
}