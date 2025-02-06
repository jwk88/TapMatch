using System;
using UnityEngine;

namespace MyTapMatch
{
    public class InputListener : MonoBehaviour, IInputController
    {
        bool _mouseClickThisFrame;
        Vector3 _mouseScreenPos;

        public bool MouseClickThisFrame { get => _mouseClickThisFrame; set => _mouseClickThisFrame = value; }
        public Vector3 MouseScreenPos { get => _mouseScreenPos; set => _mouseScreenPos = value; }

        public void Refresh()
        {
            _mouseClickThisFrame = Input.GetMouseButtonDown(0);
            _mouseScreenPos = Input.mousePosition;
        }

        void Update()
        {
            Refresh();
        }
    }
}


