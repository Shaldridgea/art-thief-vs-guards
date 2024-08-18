using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SenseInterest : MonoBehaviour
{
    [SerializeField]
    private bool isSuspicious;

    public bool IsSuspicious => isSuspicious;

    [SerializeField]
    private int priority;

    [SerializeField]
    public int Priority => priority;

    [SerializeField]
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
