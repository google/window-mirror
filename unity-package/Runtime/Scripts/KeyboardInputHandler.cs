// <copyright file="ArcCylinderSpace.cs" company="Google LLC">
//
// Copyright 2023 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace Google.XR.WindowShare
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    ///< summary>
    /// Utility to capture key strokes and generate UI events.
    ///</summary>
    public class KeyboardInputHandler : MonoBehaviour
    {
        public CylindricalUiCollider uiCollider;

        void Update()
        {
            int pressedKey = GetPressedKey();
            if (pressedKey > 0)
            {
                Debug.Log("Pressed key: " + pressedKey);
                uiCollider.HandleKeyPress(UIEventsTypes.KEYSTROKE, pressedKey);
            }
        }

        int GetPressedKey()
        {
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    return KeyCodeToASCII(keyCode);
                }
            }
            return -1;
        }

        int KeyCodeToASCII(KeyCode key)
        {
            if ((key >= KeyCode.A && key <= KeyCode.Z) ||
                (key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9))
            {

                return (int)key;
            }
            else
            {
                switch (key)
                {
                case KeyCode.Space:
                    return (int)' ';
                case KeyCode.Keypad0:
                case KeyCode.Alpha0:
                    return (int)'0';
                case KeyCode.Keypad1:
                case KeyCode.Alpha1:
                    return (int)'1';
                case KeyCode.Keypad2:
                case KeyCode.Alpha2:
                    return (int)'2';
                case KeyCode.Keypad3:
                case KeyCode.Alpha3:
                    return (int)'3';
                case KeyCode.Keypad4:
                case KeyCode.Alpha4:
                    return (int)'4';
                case KeyCode.Keypad5:
                case KeyCode.Alpha5:
                    return (int)'5';
                case KeyCode.Keypad6:
                case KeyCode.Alpha6:
                    return (int)'6';
                case KeyCode.Keypad7:
                case KeyCode.Alpha7:
                    return (int)'7';
                case KeyCode.Keypad8:
                case KeyCode.Alpha8:
                    return (int)'8';
                case KeyCode.Keypad9:
                case KeyCode.Alpha9:
                    return (int)'9';
                case KeyCode.Minus:
                case KeyCode.KeypadMinus:
                    return (int)'-';
                case KeyCode.Plus:
                case KeyCode.KeypadPlus:
                    return (int)'+';
                case KeyCode.Equals:
                case KeyCode.KeypadEquals:
                    return (int)'=';
                case KeyCode.Slash:
                case KeyCode.KeypadDivide:
                    return (int)'/';
                case KeyCode.Backslash:
                    return (int)'\\';
                case KeyCode.Period:
                case KeyCode.KeypadPeriod:
                    return (int)'.';
                case KeyCode.Comma:
                    return (int)',';
                case KeyCode.Semicolon:
                    return (int)';';
                case KeyCode.Colon:
                    return (int)':';
                case KeyCode.Exclaim:
                    return (int)'!';
                case KeyCode.Question:
                    return (int)'?';
                case KeyCode.Quote:
                    return (int)'"';
                case KeyCode.DoubleQuote:
                    return (int)'"';
                case KeyCode.LeftBracket:
                    return (int)'[';
                case KeyCode.RightBracket:
                    return (int)']';
                case KeyCode.LeftParen:
                    return (int)'(';
                case KeyCode.RightParen:
                    return (int)')';
                case KeyCode.Backspace:
                    return 08;
                case KeyCode.LeftControl or KeyCode.RightControl:
                    return 17;
                case KeyCode.KeypadEnter or KeyCode.Return:
                    return 13;
                case KeyCode.LeftArrow:
                    return 255;
                case KeyCode.RightArrow:
                    return 254;
                case KeyCode.UpArrow:
                    return 253;
                case KeyCode.DownArrow:
                    return 252;
                default:
                    return -1;
                }
            }
        }
    }
}