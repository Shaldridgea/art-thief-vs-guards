using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// Base class for actions used by the Utility AI agent
/// </summary>
public abstract class UtilityAction
{
    /// <summary>
    /// Inner class to take the raw blackboard value of a motive
    /// and transform it by mapping it to a curve to describe
    /// its importance by score
    /// </summary>
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

    /// <summary>
    /// Used to calculate the overall score for this action based on its associated motives
    /// </summary>
    public ScoreCurve[] ScoreCurves => scoreCurves;

    /// <summary>
    /// The score of this action to compare how important/desirable it is to perform against other actions
    /// </summary>
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
        // Calculate our utility score by adding up all our motives
        float utility = 0f;
        for (int i = 0; i < scoreCurves.Length; ++i)
        {
            // Get the value of this motive when evaluated on its curve
            float nextValue = scoreCurves[i].GetValue(board);

            utility += nextValue;
        }
        Score = utility;
        return Score;
    }

    public abstract void EnterAction(ThiefAgent thief);

    public abstract void PerformAction(ThiefAgent thief);

    public abstract void ExitAction(ThiefAgent thief);

#if UNITY_EDITOR
    public delegate void DebugDrawCallback();

    public virtual void OnSceneGUI() { }
#endif
}