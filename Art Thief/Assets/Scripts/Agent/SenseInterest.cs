using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SenseInterest : MonoBehaviour
{
    [SerializeField]
    private bool isSuspicious;

    public bool IsSuspicious => isSuspicious;

    [SerializeField]
    private GameObject owner;

    public GameObject Owner => owner;

    [SerializeField]
    private Consts.Team ownerTeam;

    public Consts.Team OwnerTeam => ownerTeam;

    public void SetSuspicious(bool newSuspicious) => isSuspicious = newSuspicious;
}
