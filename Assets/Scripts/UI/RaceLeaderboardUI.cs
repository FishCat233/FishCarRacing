using System.Linq;
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
    }

    private void Update()
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

        var sorted = raceRuleSystem.Racers
            .Where(r => r != null)
            .OrderBy(r => r.IsFinished ? 0 : 1)
            .ThenBy(r => r.IsFinished ? r.FinishRaceTime : float.MaxValue)
            .ThenByDescending(r => r.CurrentLap)
            .ThenByDescending(r => r.LastCheckpointIndex)
            .Take(Mathf.Max(1, maxRows))
            .ToList();

        if (sorted.Count == 0)
        {
            leaderboardText.text = "Leaderboard\nNo racers";
            return;
        }

        string text = "Leaderboard";
        for (int i = 0; i < sorted.Count; i++)
        {
            RacerProgress racer = sorted[i];
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
}
