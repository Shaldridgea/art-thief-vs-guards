using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundInterest : SenseInterest
{
    [SerializeField]
    private AudioSource[] sources;

    [SerializeField]
    private SphereCollider trigger;

    public float TriggerRadius => trigger.radius;

    [SerializeField]
    private bool testing;

    public bool IsOngoing => trigger.enabled;

    private int sourceIndex;

    // Start is called before the first frame update
    void Start()
    {
        // Set our trigger to off by default
        if(!testing)
            trigger.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        bool isAgent = other.CompareTag("Thief") || other.CompareTag("Guard");
        // If this sound trigger hits a thief or guard, notify it
        if (isAgent)
            if (other.TryGetComponent(out SensoryModule senses))
                if(senses.IsSoundHeard(this))
                    senses.NotifySound(this);
    }

    public void PlaySound()
    {
        sources[sourceIndex].Play();

        // Set trigger radius to match our currently playing source
        trigger.radius = sources[sourceIndex].maxDistance;
        trigger.enabled = true;

        // Stop any currently running coroutines on this sound trigger
        StopAllCoroutines();
        // Start a coroutine that will turn the trigger off when the sound ends
        StartCoroutine(SoundStopped(sourceIndex));

        // Loop through sources if we have multiple
        ++sourceIndex;
        sourceIndex %= sources.Length;
    }

    private IEnumerator SoundStopped(int index)
    {
        yield return new WaitForSeconds(sources[index].clip.length);
        trigger.enabled = false;
    }

    private void Reset()
    {
        if(!TryGetComponent(out AudioSource audio))
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            sources = new AudioSource[] { source };
        }

        if(!TryGetComponent(out SphereCollider sphere))
        {
            trigger = gameObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
        }

        if(!TryGetComponent(out Rigidbody rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        Agent parentAgent = GetComponentInParent<Agent>();
        if(parentAgent != null)
            Owner = parentAgent.gameObject;
        SetTeam(Consts.Team.NEUTRAL);
        gameObject.layer = LayerMask.NameToLayer("Interest");
    }
}