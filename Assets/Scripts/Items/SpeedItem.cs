using FishCarRacing.Player;
using UnityEngine;

public class SpeedItem : MonoBehaviour
{
    [Header("加成")]
    [SerializeField] private float addMaxSpeed = 100f;
    [SerializeField] private float addAcceleration = 20f;
    [SerializeField] private float addTime = 5f;
    
    private bool used;

    private void OnTriggerStay(Collider other)
    {
        if (used) return;

        if (other.CompareTag("Player"))
        {
            IncreaseSpeed(other.GetComponent<CarController>());
        }
    }

    private void IncreaseSpeed(CarController controller)
    {
        controller.ApplySpeedBurst(addMaxSpeed, addAcceleration, addTime);
        used = true;
        Destroy(gameObject);
    }
}
