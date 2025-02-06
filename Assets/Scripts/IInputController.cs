using UnityEngine;

namespace MyTapMatch
{
    public interface IInputController
    {
        bool MouseClickThisFrame { get; set; }
        Vector3 MouseScreenPos { get; set; }
        void Refresh();
    }
}


