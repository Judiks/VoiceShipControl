using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace VoiceShipControl.Shared
{
    public class KeyBindingHelper
    {
        public static int GetMouseButton(KeyCode code)
        {
            switch (code)
            {
                case KeyCode.Mouse0:
                    {
                        return (int)MouseButton.Left;
                    }
                case KeyCode.Mouse1:
                    {
                        return (int)MouseButton.Right;
                    }
                case KeyCode.Mouse2:
                    {
                        return (int)MouseButton.Middle;
                    }
                case KeyCode.Mouse3:
                    {
                        return (int)MouseButton.Forward;
                    }
                case KeyCode.Mouse4:
                    {
                        return (int)MouseButton.Back;
                    }
                default:
                    {
                        return -1;
                    }
            }
        }
    }
}
