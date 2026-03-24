using UnityEngine;
using UnityEngine.InputSystem;

namespace FishCarRacing.Player
{
    public class CarInput
    {
        public bool CanInput { get; set; }
        
        public float MoveInput { get; private set; }
        public float TurnInput{ get; private set; }
        public float DriftInput{ get; private set; }


        public CarInput()
        {
            DriftInput = 1f;
            CanInput = true;
        }
        
        public void HandleInput()
        {
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