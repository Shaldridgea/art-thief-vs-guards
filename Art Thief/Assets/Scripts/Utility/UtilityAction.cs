using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public abstract class UtilityAction
{
    [System.Serializable]
    public class MotiveUtility
    {
        [SerializeField]
        private string motiveName;

        public string MotiveName => motiveName;

        public float ProjectedValueChange;

        [SerializeField]
        private AnimationCurve utilityCurve;

        public AnimationCurve Curve => utilityCurve;

        [SerializeField]
        private float weight = 1f;

        public float Weight => weight;

        public float MinValue { get; private set; }
        public float MaxValue { get; private set; }

        public void Init()
        {
            if (utilityCurve == null)
                return;

            MinValue = utilityCurve.keys[0].time;
            MaxValue = utilityCurve.keys[utilityCurve.keys.Length - 1].time;
        }
    }

    protected Consts.UtilityActionType type;

    public string Name => type.ToString();

    protected MotiveUtility[] motiveUtils;

    protected MotiveValue[] motiveValues;

    protected Coroutine actionCoroutine;

    public float Score { get; private set; }

    public UtilityAction(ActionData newData, MotiveValue[] newMotives)
    {
        if (newData == null || newMotives == null)
            return;

        // Set action data
        type = newData.Action;
        motiveUtils = newData.Motives;
        // Initialise values in our motives
        for (int i = 0; i < motiveUtils.Length; ++i)
            motiveUtils[i].Init();
        motiveValues = newMotives;
    }

    public float EvaluateUtility()
    {
        // Calculate our utility by averaging across all our motives
        float utility = 0f;
        for (int i = 0; i < motiveValues.Length; ++i)
        {
            // Get the value of this motive and evaluate it on its curve, multiplying by the weight
            float nextValue = motiveUtils[i].Curve.Evaluate(
                Mathf.Clamp(motiveValues[i].Value, motiveUtils[i].MinValue, motiveUtils[i].MaxValue))
                * motiveUtils[i].Weight;

            utility += nextValue;
        }
        Score = utility / motiveValues.Length;
        return Score;
    }

    public abstract void PerformAction(ThiefAgent spy);

    public abstract void StopAction(ThiefAgent spy);

    public abstract UtilityAction GetNewInstance(ActionData newData, MotiveValue[] newMotives);
}