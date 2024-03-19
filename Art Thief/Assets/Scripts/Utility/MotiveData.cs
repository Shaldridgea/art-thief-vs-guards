using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "New Motive", menuName = "Motive Data")]
public class MotiveData : ScriptableObject
{
    [SerializeField]
    private string motiveName;

    public string Motive => motiveName;

    [SerializeField]
    private string blackboardValue;

    public string BlackboardKey => blackboardValue;
}