using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
    [SerializeField]
    private AudioSource[] sources;

    [SerializeField]
    private SphereCollider trigger;

    [SerializeField]
    private LayerMask soundMask;

    [SerializeField]
    private bool urgent;

    public bool Urgent => urgent;

    [SerializeField]
    private Consts.AgentSource agentSource;

    public Consts.AgentSource AgentSource => agentSource;

    public bool IsOngoing => trigger.enabled;

    private int sourceIndex;

    // Start is called before the first frame update
    void Start()
    {
        // Set our trigger radius to be the same as our sound radius
        trigger.radius = sources[0].maxDistance;
        // Set all our audio sources to have the same distance radii if we have more than one source
        if(sources.Length > 1)
            for(int i = 1; i < sources.Length; ++i)
            {
                sources[i].minDistance = sources[0].minDistance;
                sources[i].maxDistance = sources[0].maxDistance;
            }
        // Set our trigger to off by default
        trigger.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        bool isSpy = other.CompareTag("Spy");
        bool isGuard = other.CompareTag("Guard");
        // If this sound trigger is in the vicinity of a spy or guard
        if(isSpy || isGuard)
        {
            bool shouldNotify = true;
            // If the agent we found is further than the min distance
            if (Vector3.Distance(other.transform.position, transform.position) > sources[0].minDistance)
                if (Physics.Linecast(transform.position, other.transform.position, soundMask)) // If there is a wall blocking sound
                    shouldNotify = false;

            // If there is no wall blocking the sound or the agent is within the minimum distance, notify them of the sound
            if(shouldNotify)
            {
                Agent agent = other.GetComponent<Agent>();
                agent.NotifySound(this);
            }
        }
    }

    public void PlaySound()
    {
        // Play the audio source
        sources[sourceIndex].Play();
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