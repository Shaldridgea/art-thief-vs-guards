using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SensoryModule : MonoBehaviour
{
    [SerializeField]
    protected Agent owner;

    [SerializeField]
    protected float losCheckInterval;

    [SerializeField]
    protected float awarenessIncrease;

    public float AwarenessIncrease => awarenessIncrease;

    public delegate void SenseDelegate();

    public event SenseDelegate EnemyFound;

    public event SenseDelegate EnemyLost;

    public event SenseDelegate SoundHappened;

    protected float awareness;

    protected float losTimer;

    public abstract void SoundHeard(SoundTrigger sound);

    protected void TriggerEnemyFound() => EnemyFound?.Invoke();

    protected void TriggerEnemyLost() => EnemyLost?.Invoke();

    protected void TriggerSoundHappened() => SoundHappened?.Invoke();

    public const float INTEREST_RADIUS = 5f;

    protected List<GameObject> FindNearbyInterests(string interestTag)
    {
        Collider[] nearbyInterests =
        Physics.OverlapSphere(
        transform.position,
        INTEREST_RADIUS,
        LayerMask.GetMask("Interest"),
        QueryTriggerInteraction.Collide);

        List<GameObject> desiredInterests = new List<GameObject>();
        foreach(Collider c in nearbyInterests)
        {
            if (c.CompareTag(interestTag))
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