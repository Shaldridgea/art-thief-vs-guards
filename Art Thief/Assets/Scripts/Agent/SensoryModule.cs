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
}