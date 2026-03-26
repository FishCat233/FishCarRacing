using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RaceLeaderboardUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RaceRuleSystem raceRuleSystem;
    [SerializeField] private RacerProgress localRacer;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI leaderboardText;
    [SerializeField] private TextMeshProUGUI lapText;

    [Header("Display")]
    [SerializeField] private int maxRows = 8;

    private readonly List<RacerProgress> _sortedRacers = new List<RacerProgress>(16);

    private void Awake()
    {
        if (raceRuleSystem == null)
        {
            raceRuleSystem = FindFirstObjectByType<RaceRuleSystem>();
        }

        if (localRacer == null)
        {
            localRacer = FindFirstObjectByType<RacerProgress>();
        }

        RefreshUI();
    }

    private void OnEnable()
    {
        SubscribeEvents();
        RefreshUI();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void RefreshUI()
    {
        if (raceRuleSystem == null)
        {
            if (leaderboardText != null) leaderboardText.text = "Leaderboard: N/A";
            if (lapText != null) lapText.text = "Lap: N/A";
            return;
        }

        UpdateLeaderboardText();
        UpdateLapText();
    }

    private void UpdateLeaderboardText()
    {
        if (leaderboardText == null)
        {
            return;
        }

        _sortedRacers.Clear();
        var racers = raceRuleSystem.Racers;
        for (int i = 0; i < racers.Count; i++)
        {
            RacerProgress racer = racers[i];
            if (racer != null)
            {
                _sortedRacers.Add(racer);
            }
        }

        _sortedRacers.Sort(CompareRacers);

        if (_sortedRacers.Count == 0)
        {
            leaderboardText.text = "Leaderboard\nNo racers";
            return;
        }

        string text = "Leaderboard";

        int rowCount = Mathf.Min(Mathf.Max(1, maxRows), _sortedRacers.Count);
        for (int i = 0; i < rowCount; i++)
        {
            RacerProgress racer = _sortedRacers[i];
            string finishTag = racer.IsFinished ? " [FIN]" : string.Empty;
            text += $"\n{i + 1}. {racer.RacerId}  Lap {racer.CurrentLap}/{raceRuleSystem.TargetLapCount}{finishTag}";
        }

        leaderboardText.text = text;
    }

    private void UpdateLapText()
    {
        if (lapText == null)
        {
            return;
        }

        if (localRacer == null)
        {
            lapText.text = $"Lap: 0/{raceRuleSystem.TargetLapCount}";
            return;
        }

        string finishText = localRacer.IsFinished ? "  FINISHED" : string.Empty;
        lapText.text = $"Lap: {localRacer.CurrentLap}/{raceRuleSystem.TargetLapCount}{finishText}";
    }

    private static int CompareRacers(RacerProgress a, RacerProgress b)
    {
        if (a == b)
        {
            return 0;
        }

        if (a.IsFinished != b.IsFinished)
        {
            return a.IsFinished ? -1 : 1;
        }

        if (a.IsFinished && b.IsFinished)
        {
            int finishCompare = a.FinishRaceTime.CompareTo(b.FinishRaceTime);
            if (finishCompare != 0)
            {
                return finishCompare;
            }
        }

        int lapCompare = b.CurrentLap.CompareTo(a.CurrentLap);
        if (lapCompare != 0)
        {
            return lapCompare;
        }

        return b.LastCheckpointIndex.CompareTo(a.LastCheckpointIndex);
    }

    private void SubscribeEvents()
    {
        if (raceRuleSystem == null)
        {
            return;
        }

        raceRuleSystem.RacerProgressChanged -= OnRacerProgressChanged;
        raceRuleSystem.RacerProgressChanged += OnRacerProgressChanged;
        raceRuleSystem.LapCompleted -= OnLapCompleted;
        raceRuleSystem.LapCompleted += OnLapCompleted;
        raceRuleSystem.RacerFinished -= OnRacerFinished;
        raceRuleSystem.RacerFinished += OnRacerFinished;
    }

    private void UnsubscribeEvents()
    {
        if (raceRuleSystem == null)
        {
            return;
        }

        raceRuleSystem.RacerProgressChanged -= OnRacerProgressChanged;
        raceRuleSystem.LapCompleted -= OnLapCompleted;
        raceRuleSystem.RacerFinished -= OnRacerFinished;
    }

    private void OnRacerProgressChanged(RacerProgress racer)
    {
        RefreshUI();
    }

    private void OnLapCompleted(RacerProgress racer, int lap)
    {
        RefreshUI();
    }

    private void OnRacerFinished(RacerProgress racer)
    {
        RefreshUI();
    }
}
