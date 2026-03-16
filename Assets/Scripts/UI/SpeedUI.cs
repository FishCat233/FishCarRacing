using FishCarRacing.Player;
using TMPro;
using UnityEngine;

public class SpeedUI : MonoBehaviour
{
    [SerializeField] private CarController controller;
    [SerializeField] private TextMeshProUGUI speedText;

    private void Awake()
    {
        if (controller == null) controller = FindObjectOfType<CarController>();
        if (speedText == null) speedText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        
        speedText.text = ((int)controller.SpeedKmh).ToString();
    }
}
