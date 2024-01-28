// <copyright file="VRScreens.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    ///<summary>
    /// This manages the List<IVRScreen> which is the list containing
    /// the VRScreen visible within the cylindrical space.
    /// There are two ways to use this class.
    /// Use the list setter to directly provide IVRScreens to manage.
    /// Or use vrScreenListObject in the editor to provide a
    /// gameobject containing a list of IVRScreens.
    ///</summary>
    public class VRScreens : MonoBehaviour
    {
        [HideInInspector]
        public List<IVRScreen> list
        {
            get {
                return _list;
            }
            set {
                // Call Dispose on any list that are in _list but not in value
                foreach (var existingRect in _list)
                {
                    if (!value.Contains(existingRect))
                    {
                        Destroy(existingRect.plane2D);
                        Destroy(existingRect.screen);
                    }
                }
                _list = value;
            }
        }

        [HideInInspector]
        public Object vrScreenListObject {
            get {
                    return _vrScreenListObject;
                }
            set {
                    if (value != null && !(value is IVRScreenList))
                    {
                        Debug.LogError("The assigned object must implement the IVRScreenList interface.");
                    }
                    _vrScreenListObject = value;
                }
        }

        private IVRScreenList _vrScreenList;
        private List<IVRScreen> _list = new List<IVRScreen>();
        private Object _vrScreenListObject;

        public void LoadVRScreens()
        {
            AssignList();

            if (_vrScreenList == null)
            {
                Debug.LogError("VRScreenList is not assigned!");
                return;
            }

            foreach (IVRScreen screen in _vrScreenList.GetScreens())
            {
                list.Add(screen);

                if (screen.textureData != null && screen.textureData.Length > 0)
                {
                    screen.tex = new Texture2D(2, 2);
                    screen.tex.LoadImage(screen.textureData);
                }
            }
        }

        public void SaveVRScreens()
        {
           
            AssignList();

            if (_vrScreenList == null)
            {
                Debug.LogError("VRScreenList is not assigned!");
                return;
            }

            _vrScreenList.ClearScreens();
            EncodeTextures();
            _vrScreenList.SetScreens(_list);
#if UNITY_EDITOR
            // Save the changes to the scriptable object
            EditorUtility.SetDirty(_vrScreenList as UnityEngine.Object);
            AssetDatabase.SaveAssets();
#endif
        }

        private void EncodeTextures()
        {
            foreach (IVRScreen screen in _list)
            {
                if (screen.tex != null)
                {
                    screen.textureData = screen.tex.EncodeToJPG(); 
                }
            }
        }

        // Method to check if a point (angle, height) is within any VRScreen
        public IVRScreen IsPointInsideAnyVRScreen(Vector2 point)
        {
            foreach (var VRScreen in _list)
            {
                if (IsPointInsideVRScreen(VRScreen, point))
                {
                    return VRScreen;  // Point is inside this VRScreen
                }
            }
            return null;  // Point is not inside any VRScreen
        }

        private void AssignList() {
            // Cast the Object to the IVRScreenList interface
            _vrScreenList = _vrScreenListObject as IVRScreenList;
        }

        private bool IsPointInsideVRScreen(IVRScreen rect, Vector2 point)
        {
            // Check if point is within the VRScreen bounds
            Vector2 bottomLeft = rect.BottomLeft();
            Vector2 topRight = rect.TopRight();

            return point.x >= bottomLeft.x && point.x <= topRight.x && point.y >= bottomLeft.y &&
                   point.y <= topRight.y;
        }

        void OnDestroy()
        {
            list.Clear();
        }
    }
}