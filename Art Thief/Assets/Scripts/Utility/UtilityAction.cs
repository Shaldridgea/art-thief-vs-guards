using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public abstract class UtilityAction
{
    [System.Serializable]
    public class ScoreCurve
    {
        [SerializeField]
        private MotiveData motiveSource;

        public MotiveData MotiveSource => motiveSource;

        [SerializeField]
        private AnimationCurve curve;

        public AnimationCurve Curve => curve;

        public float MinValue { get; private set; }
        public float MaxValue { get; private set; }

        public void Init()
        {
            if (curve == null)
                return;

            MinValue = curve.keys[0].time;
            MaxValue = curve.keys[curve.keys.Length - 1].time;
            curve.preWrapMode = WrapMode.ClampForever;
            curve.postWrapMode = WrapMode.ClampForever;
        }

        public float GetValue(Blackboard board) => curve.Evaluate(motiveSource.GetInsistence(board));
    }

    protected Consts.UtilityActionType type;

    public string Name => type.ToString();

    protected ScoreCurve[] scoreCurves;

    protected Coroutine actionCoroutine;

    public float Score { get; private set; }

    public UtilityAction(ActionData newData)
    {
        if (newData == null)
            return;

        // Set action data
        type = newData.Action;
        scoreCurves = newData.Motives;
        // Initialise values in our motives
        for (int i = 0; i < scoreCurves.Length; ++i)
            scoreCurves[i].Init();
    }

    public float CalculateScore(Blackboard board)
    {
        // Calculate our utility by averaging across all our motives
        float utility = 0f;
        for (int i = 0; i < scoreCurves.Length; ++i)
        {
            // Get the value of this motive and evaluate it on its curve, multiplying by the weight
            float nextValue = scoreCurves[i].GetValue(board);

            utility += nextValue;
        }
        Score = utility;
        return Score;
    }

    public abstract void PerformAction(ThiefAgent thief);

    public abstract void StopAction(ThiefAgent thief);
}