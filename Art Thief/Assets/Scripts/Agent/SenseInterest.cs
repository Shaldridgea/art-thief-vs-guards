using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interest component to describe sensory events detected by AI agent's senses
/// </summary>
public class SenseInterest : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Is this interest notable to Guards and should be investigated?")]
    private bool isSuspicious;

    public bool IsSuspicious => isSuspicious;

    [SerializeField]
    private int priority;

    [SerializeField]
    [Tooltip("How important this interest is compared to other interests")]
    public int Priority => priority;

    [SerializeField]
    [Tooltip("Whether this interest should be noticed every time it's encountered, or only once")]
    private bool alwaysImportant;

    public bool AlwaysImportant => alwaysImportant;

    [SerializeField]
    private GameObject owner;

    public GameObject Owner { get => owner; protected set => owner = value; }

    [SerializeField]
    private Consts.Team ownerTeam;

    public Consts.Team OwnerTeam => ownerTeam;

    public void SetSuspicious(bool newSuspicious) => isSuspicious = newSuspicious;

    public void SetTeam(Consts.Team newTeam) => ownerTeam = newTeam;
}
