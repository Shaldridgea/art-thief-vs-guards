﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SensoryModule : MonoBehaviour
{
    [SerializeField]
    protected float losCheckInterval;

    [SerializeField]
    protected float awarenessIncrease;

    public float AwarenessIncrease => awarenessIncrease;

    public delegate void SenseDelegate();

    public event SenseDelegate EnemyFound;

    public event SenseDelegate EnemyLost;

    public event SenseDelegate SoundHappened;

    protected Agent owner;

    protected float awareness;

    protected float losTimer;

    virtual protected void Start()
    {
        owner = GetComponent<Agent>();
    }

    public abstract void SoundHeard(SoundTrigger sound);

    protected void TriggerEnemyFound() => EnemyFound?.Invoke();

    protected void TriggerEnemyLost() => EnemyLost?.Invoke();

    protected void TriggerSoundHappened() => SoundHappened?.Invoke();

    public const float INTEREST_RADIUS = 6f;

    protected List<GameObject> FindNearbyInterests(string interestTag)
    {
        Collider[] nearbyInterests =
        Physics.OverlapSphere(
        transform.position,
        INTEREST_RADIUS,
        LayerMask.GetMask("Interest", "Thief", "Guard"),
        QueryTriggerInteraction.Collide);

        List<GameObject> desiredInterests = new List<GameObject>();
        foreach(Collider c in nearbyInterests)
        {
            if (c.CompareTag(interestTag) && c.gameObject != owner.gameObject)
                desiredInterests.Add(c.gameObject);
        }
        return desiredInterests;
    }

    public bool StoreNearbyInterests(string interestTag, Blackboard targetBoard, string listKey)
    {
        var interests = FindNearbyInterests(interestTag);
        if(interests.Count > 0)
        {
            var existingList = targetBoard.GetVariable<List<GameObject>>(listKey);
            if (existingList == null)
                targetBoard.SetVariable(listKey, interests);
            else
            {
                existingList.Clear();
                existingList.AddRange(interests);
            }
            return true;
        }
        return false;
    }
}