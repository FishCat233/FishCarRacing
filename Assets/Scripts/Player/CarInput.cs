using UnityEngine;
using UnityEngine.InputSystem;

namespace FishCarRacing.Player
{
    public class CarInput : MonoBehaviour
    {
        public bool CanInput { get; private set; }
        
        public float MoveInput { get; private set; }
        public float TurnInput{ get; private set; }
        public float DriftInput{ get; private set; }

        private void Awake()
        {
            DriftInput = 1f;
            CanInput = true;
        }

        public void SetCanInput(bool canInput)
        {
            CanInput = canInput;
            if (!CanInput)
            {
                MoveInput = 0f;
                TurnInput = 0f;
            }
        }
        
        public void HandleInput()
        {
            if (!CanInput)
            {
                MoveInput = 0f;
                TurnInput = 0f;
                return;
            }

            var wKey = Keyboard.current.wKey.isPressed;
            var sKey = Keyboard.current.sKey.isPressed;
            if (wKey) MoveInput = 1f;
            else if (sKey) MoveInput = -1f;
            else MoveInput = 0f;

            var aKey = Keyboard.current.aKey.isPressed;
            var dKey = Keyboard.current.dKey.isPressed;
            if (aKey) TurnInput = -1f;
            else if (dKey) TurnInput = 1f;
            else TurnInput = 0f;

            DriftInput = TurnInput != 0f ? TurnInput : DriftInput;
        }
    }
}