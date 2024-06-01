using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SensoryModule : MonoBehaviour
{
    [SerializeField]
    protected LayerMask losMask;

    public LayerMask LosMask => losMask;

    [SerializeField]
    protected float losCheckInterval;

    [SerializeField]
    protected float awarenessChange;

    public float AwarenessIncrease => awarenessChange;

    public delegate void SenseDelegate(SenseInterest interest);

    public event SenseDelegate EnemyFound;

    public event SenseDelegate EnemyLost;

    public event SenseDelegate VisualFound;

    public event SenseDelegate VisualLost;

    public event SenseDelegate SoundHeard;

    protected Agent owner;

    protected float awareness;

    protected float losTimer;

    protected virtual void Start()
    {
        owner = GetComponent<Agent>();
    }

    public virtual void NotifySound(SenseInterest sound)
    {
        SoundHeard?.Invoke(sound);
    }

    public virtual void NotifyVisualFound(SenseInterest visual)
    {
        VisualFound?.Invoke(visual);
    }

    public virtual void NotifyVisualLost(SenseInterest visual)
    {
        VisualLost?.Invoke(visual);
    }

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