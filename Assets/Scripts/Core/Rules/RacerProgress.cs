using UnityEngine;

[DisallowMultipleComponent]
public class RacerProgress : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string racerId;

    [Header("Runtime (Read Only)")]
    [SerializeField] private int currentLap;
    [SerializeField] private int score;
    [SerializeField] private bool isFinished;
    [SerializeField] private float finishRaceTime;
    [SerializeField] private int nextCheckpointIndex = 1;
    [SerializeField] private int lastCheckpointIndex = -1;
    [SerializeField] private float lastCheckpointTriggerTime = -999f;

    public string RacerId => string.IsNullOrWhiteSpace(racerId) ? gameObject.name : racerId;
    public int CurrentLap => currentLap;
    public int Score => score;
    public bool IsFinished => isFinished;
    public float FinishRaceTime => finishRaceTime;
    public int NextCheckpointIndex => nextCheckpointIndex;
    public int LastCheckpointIndex => lastCheckpointIndex;
    public float LastCheckpointTriggerTime => lastCheckpointTriggerTime;

    private void Reset()
    {
        if (string.IsNullOrWhiteSpace(racerId))
        {
            racerId = gameObject.name;
        }
    }

    public void ResetForRace(int firstCheckpointIndex)
    {
        currentLap = 0;
        score = 0;
        isFinished = false;
        finishRaceTime = 0f;
        nextCheckpointIndex = firstCheckpointIndex;
        lastCheckpointIndex = -1;
        lastCheckpointTriggerTime = -999f;
    }

    public void MarkCheckpointPassed(int checkpointIndex, float triggerTime)
    {
        lastCheckpointIndex = checkpointIndex;
        lastCheckpointTriggerTime = triggerTime;
    }

    public void SetNextCheckpointIndex(int checkpointIndex)
    {
        nextCheckpointIndex = checkpointIndex;
    }

    public int AddLapAndScore(int addScore)
    {
        currentLap += 1;
        score += addScore;
        return currentLap;
    }

    public void MarkFinished(float raceTime)
    {
        isFinished = true;
        finishRaceTime = raceTime;
    }
}
