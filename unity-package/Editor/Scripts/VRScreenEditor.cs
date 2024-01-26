// <copyright file="VRScreenEditor.cs" company="Google LLC">
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

namespace Google.XR.WindowMirror
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(VRScreens))]
    ///<summary>
    /// This loads VR screen data for testing within 
    /// editor without receiver or playmode.
    ///</summary>
    public class VRScreensEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            VRScreens vrScreens = (VRScreens)target;

            // Create a field where only objects implementing IVRScreenList can be assigned
            vrScreens.vrScreenListObject = EditorGUILayout.ObjectField(
                "VR Screen List", vrScreens.vrScreenListObject, typeof(ScriptableObject), true);

            if (GUILayout.Button("Load VR Screens"))
            {
                vrScreens.LoadVRScreens();
            }

            if (GUILayout.Button("Save VR Screens"))
            {
                vrScreens.SaveVRScreens();
            }
        }
    }
}
