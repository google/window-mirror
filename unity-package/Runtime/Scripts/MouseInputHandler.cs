// <copyright file="MouseInputHandler.cs" company="Google LLC">
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
    using UnityEngine;

    ///< summary>
    /// Utility to capture mouse events and pass them to the uicollider for
    /// coordinates remapping.
    ///</summary>
    public class MouseInputHandler : MonoBehaviour
    {
        public CylindricalUiCollider uiCollider;

        private float doubleClickTimeLimit = 0.25f;
        private float lastLeftClickTime = -1f;

        public GameObject cursorPrefab;
        private GameObject currentCursor;

        void Update()
        {
            if (Input.GetMouseButtonDown(0))  // Left button down
            {
                CheckRaycastHit(UIEventsTypes.LEFT_BUTTON_DOWN);

                // Checking for double click
                if (Time.time - lastLeftClickTime < doubleClickTimeLimit)
                {
                    CheckRaycastHit(UIEventsTypes.LEFT_DOUBLE_CLICK);
                }
                lastLeftClickTime = Time.time;
            }

            if (Input.GetMouseButtonDown(1))  // Right button down
            {
                CheckRaycastHit(UIEventsTypes.RIGHT_BUTTON_DOWN);
            }

            if (Input.GetMouseButtonDown(2))  // Middle button down
            {
                CheckRaycastHit(UIEventsTypes.MIDDLE_BUTTON_DOWN);
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                scroll = scroll * 10;
                CheckRaycastHit(UIEventsTypes.SCROLL, (int)scroll);
            }

            ContinuosCheckRaycastHit();
        }

        private void CheckRaycastHit(UIEventsTypes eventType, int value = 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                uiCollider.HandleHit(hit.point, eventType, value);
            }
        }

        private void ContinuosCheckRaycastHit()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // uiCollider.HandleContinuousHit(hit.point);
                UpdateCursorPosition(hit);
            }
        }

        private void UpdateCursorPosition(RaycastHit hit)
        {
            if (currentCursor == null)
            {
                currentCursor = Instantiate(cursorPrefab);
            }
            currentCursor.transform.position = hit.point;
        }
    }
}