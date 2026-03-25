using UnityEngine;

public class CheckPointTrigger : MonoBehaviour
{
    [Header("Checkpoint")]
    [SerializeField] private int checkpointIndex;
    [SerializeField] private bool isFinishLine;

    [Header("Filter")]
    [SerializeField] private string racerTag = "Player";
    [SerializeField] private bool requireTag = false;

    public int CheckpointIndex => checkpointIndex;
    public bool IsFinishLine => isFinishLine;

    private void OnTriggerEnter(Collider other)
    {
        if (requireTag && !other.CompareTag(racerTag))
        {
            return;
        }

        if (RaceManager.Instance == null)
        {
            return;
        }

        RaceManager.Instance.TryProcessCheckpoint(this, other);
    }
}
