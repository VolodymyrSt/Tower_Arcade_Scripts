using System;
using UnityEngine;

namespace Game
{
    public class GameInput : IDisposable
    {
        private readonly InputActions _action;

        public GameInput()
        {
            _action = new InputActions();
            _action.Enable();
        }

        public Vector2 GetScrollVectorNormalized() => 
            _action.Player.Zoom.ReadValue<Vector2>().normalized;

        public Vector2 GetCameraMoveVectorNormalized() => 
            _action.Player.MoveCamera.ReadValue<Vector2>().normalized;

        public void Dispose() => 
            _action.Disable();
    }
}
