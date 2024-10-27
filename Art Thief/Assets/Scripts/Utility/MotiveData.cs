using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// Data container for a Utility motive
/// </summary>
[CreateAssetMenu(fileName = "New Motive", menuName = "Motive Data")]
public class MotiveData : ScriptableObject
{
    [SerializeField]
    private string motiveName;

    /// <summary>
    /// Name/identifier for this motive
    /// </summary>
    public string Motive => motiveName;

    [SerializeField]
    private string blackboardKey;

    public string BlackboardKey => blackboardKey;

    public float GetInsistence(Blackboard board) => board.GetVariable<float>(blackboardKey);
}