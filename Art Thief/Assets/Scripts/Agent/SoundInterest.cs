using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundInterest : SenseInterest
{
    [SerializeField]
    private AudioSource[] sources;

    [SerializeField]
    private SphereCollider trigger;

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
        if(isAgent)
            if(other.TryGetComponent(out SensoryModule senses))
                senses.NotifySound(this);
    }

    public void PlaySound()
    {
        // Play the audio source
        sources[sourceIndex].Play();
        // Set trigger radius to match our currently playing source
        trigger.radius = sources[sourceIndex].maxDistance;
        trigger.enabled = true;
        // Stop any currently running coroutines on this sound trigger
        StopAllCoroutines();
        // Start a coroutine that will keep the trigger on for the duration of the sound clip
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
}