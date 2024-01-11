/// <copyright file="ArcCylinderSpace.cs" company="Google LLC">
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
    using UnityEditor;

    //<summary>
    /// This adds a button to the editor that removes the last VRScreen.
    ///</summary>
    [CustomEditor(typeof(VRScreens))]
    public class VRScreensEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Remove Last VRScreen"))
            {
                VRScreens VRScreensComponent = (VRScreens)target;
                if (VRScreensComponent.vrscreens.Count == 0)
                    return;

                var index = VRScreensComponent.vrscreens.Count - 1;
                VRScreensComponent.vrscreens[index].Dispose();
                VRScreensComponent.vrscreens.RemoveAt(index);
            }
        }
    }
}