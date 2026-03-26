using System;
using System.Collections.Generic;
using System.Linq;
using FishCarRacing.Player;
using UnityEngine;

public class RaceRuleSystem : MonoBehaviour
{
    [Header("Race Rules")]
    [SerializeField] private int targetLapCount = 3;
    [SerializeField] private float sameCheckpointCooldown = 0.2f;

    [Header("Finish Direction Check")]
    [SerializeField] [Range(-1f, 1f)] private float finishDirectionDotThreshold = 0.2f;

    [Header("References")]
    [SerializeField] private List<CheckPointTrigger> checkpoints = new List<CheckPointTrigger>();
    [SerializeField] private List<RacerProgress> racers = new List<RacerProgress>();
    [SerializeField] private bool autoDiscoverRacersOnPrepare = true;

    private bool raceActive;
    private int finishedRacerCount;

    public int TargetLapCount => targetLapCount;
    public bool RaceActive => raceActive;
    public bool AreAllRacersFinished => racers.Count > 0 && finishedRacerCount >= racers.Count;
    public IReadOnlyList<RacerProgress> Racers => racers;

    public event Action<RacerProgress> RacerFinished;
    public event Action<RacerProgress, int> LapCompleted;

    private void Awake()
    {
        NormalizeCheckpoints();
    }

    public void RegisterCheckpoint(CheckPointTrigger checkpoint)
    {
        if (checkpoint == null) return;
        if (checkpoints.Contains(checkpoint)) return;

        checkpoints.Add(checkpoint);
        NormalizeCheckpoints();
    }

    public void PrepareRace()
    {
        NormalizeCheckpoints();

        if (checkpoints.Count == 0)
        {
            Debug.LogWarning("[RaceRuleSystem] No checkpoints configured. Lap progress will not work.");
        }
        else if (!checkpoints.Any(cp => cp.IsFinishLine))
        {
            Debug.LogWarning("[RaceRuleSystem] No finish-line checkpoint found. Set one checkpoint as finish line.");
        }

        if (autoDiscoverRacersOnPrepare)
        {
            racers = FindObjectsOfType<RacerProgress>().ToList();
        }

        int first = GetFirstNonFinishCheckpointIndex();
        foreach (var racer in racers)
        {
            racer.ResetForRace(first);
        }

        raceActive = false;
        finishedRacerCount = 0;
    }

    public void StartRace()
    {
        raceActive = true;
    }

    public void StopRace()
    {
        raceActive = false;
    }

    public void DisableFinishedRacersInput()
    {
        foreach (var racer in racers)
        {
            if (racer == null || !racer.IsFinished)
            {
                continue;
            }

            CarController controller = racer.GetComponentInParent<CarController>();
            if (controller != null && controller.CanInput)
            {
                controller.SetInputEnabled(false);
            }
        }
    }

    public bool TryPassCheckpoint(CheckPointTrigger checkpoint, Collider triggerCollider, float raceElapsedTime)
    {
        if (!raceActive || checkpoint == null || triggerCollider == null)
        {
            return false;
        }

        RacerProgress racer = triggerCollider.GetComponentInParent<RacerProgress>();
        if (racer == null)
        {
            return false;
        }

        if (!racers.Contains(racer))
        {
            racers.Add(racer);
        }

        if (racer.IsFinished)
        {
            return false;
        }

        if (racer.LastCheckpointIndex == checkpoint.CheckpointIndex &&
            Time.time - racer.LastCheckpointTriggerTime < sameCheckpointCooldown)
        {
            return false;
        }

        if (checkpoint.CheckpointIndex != racer.NextCheckpointIndex)
        {
            return false;
        }
        racer.MarkCheckpointPassed(checkpoint.CheckpointIndex, Time.time);

        if (checkpoint.IsFinishLine)
        {
            int lap = racer.AddLap();
            LapCompleted?.Invoke(racer, lap);

            int next = GetFirstNonFinishCheckpointIndex();
            racer.SetNextCheckpointIndex(next);

            if (lap >= targetLapCount)
            {
                racer.MarkFinished(raceElapsedTime);
                finishedRacerCount += 1;
                RacerFinished?.Invoke(racer);
            }

            return true;
        }

        racer.SetNextCheckpointIndex(GetNextCheckpointIndex(checkpoint.CheckpointIndex));
        return true;
    }

    private void NormalizeCheckpoints()
    {
        checkpoints = checkpoints
            .Where(cp => cp != null)
            .OrderBy(cp => cp.CheckpointIndex)
            .ToList();
    }

    private int GetFirstNonFinishCheckpointIndex()
    {
        if (checkpoints.Count <= 1)
        {
            return 0;
        }

        for (int i = 0; i < checkpoints.Count; i++)
        {
            if (!checkpoints[i].IsFinishLine)
            {
                return checkpoints[i].CheckpointIndex;
            }
        }

        return checkpoints[0].CheckpointIndex;
    }

    private int GetNextCheckpointIndex(int currentIndex)
    {
        if (checkpoints.Count == 0)
        {
            return 0;
        }

        for (int i = 0; i < checkpoints.Count; i++)
        {
            if (checkpoints[i].CheckpointIndex == currentIndex)
            {
                int next = (i + 1) % checkpoints.Count;
                return checkpoints[next].CheckpointIndex;
            }
        }

        return checkpoints[0].CheckpointIndex;
    }
}
