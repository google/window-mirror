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
    using System;

    ///< summary>
    /// This class contains the types of UI events that can be sent to the.
    /// streaming application for simulation.
    ///</summary>
    public class UIEvent
    {
        public int EventType { get; private set; }
        public int Value { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int WindowId { get; private set; }
        public int[] dataArray;

        public UIEvent(UIEventsTypes eventType = UIEventsTypes.UNINITIALIZED, int value = 0,
                       int x = 0, int y = 0, int windowId = 0)
        {
            EventType = (int)eventType;
            Value = value;
            X = x;
            Y = y;
            WindowId = windowId;
            dataArray = new Int32[] { EventType, Value, X, Y, WindowId };
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[dataArray.Length * 4];

            for (int i = 0; i < dataArray.Length; i++)
            {
                byte[] byteValue = BitConverter.GetBytes(dataArray[i]);

                // Ensure little-endian format if system is big-endian
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(byteValue);
                }

                Array.Copy(byteValue, 0, bytes, i * 4, 4);
            }

            return bytes;
        }

        public void FromBytes(byte[] inbytes)
        {
            if (inbytes.Length != dataArray.Length * 4)
            {
                throw new ArgumentException("Byte array is not the correct size.");
            }

            for (int i = 0; i < dataArray.Length; i++)
            {
                dataArray[i] = BitConverter.ToInt32(inbytes, i * 4);
            }

            // Update the properties with the values from dataArray
            EventType = dataArray[0];
            Value = dataArray[1];
            X = dataArray[2];
            Y = dataArray[3];
            WindowId = dataArray[4];
        }
    }

    public enum UIEventsTypes
    {
        UNINITIALIZED = 0,
        LEFT_BUTTON_DOWN = 1,     // cv2.EVENT_LBUTTONDOWN
        RIGHT_BUTTON_DOWN = 2,    // cv2.EVENT_RBUTTONDOWN
        MIDDLE_BUTTON_DOWN = 3,   // cv2.EVENT_MBUTTONDOWN
        LEFT_DOUBLE_CLICK = 7,    // cv2.EVENT_LBUTTONDBLCLK
        RIGHT_DOUBLE_CLICK = 8,   // cv2.EVENT_RBUTTONDBLCLK
        MIDDLE_DOUBLE_CLICK = 9,  // cv2.EVENT_MBUTTONDBLCLK
        SCROLL = 10,              // cv2.EVENT_MOUSEWHEEL
        KEYSTROKE = -500
    }
}