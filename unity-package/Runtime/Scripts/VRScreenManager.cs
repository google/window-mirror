// <copyright file="VRScreenManager.cs" company="Google LLC">
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

    ///<summary>
    /// This class dispatch the texture to the correct screen (VRScreen) and
    /// triggers replacement and mesh regeneration when needed (when a texture
    /// changes dimension)
    ///</summary>
    public class VRScreenManager : MonoBehaviour
    {
        public float globalscale = 0.0008f;
        public Receiver receiver;
        public VRScreens Screens;
        public VRScreenPlacer VRScreenPlacer;
        public CylindricalMeshGenerator cylindricalMeshGenerator;
        public CylindricalUiCollider cylindricalUiCollider;

        void Start()
        {
            Initialize();
        }

        protected void Initialize(){

            // Subscribe to the event emitted by Receiver
            receiver.OnImageDataReceived += HandleImageData;

            // if ui is on create ui collider
            if (cylindricalUiCollider != null)
            {
                cylindricalUiCollider.Initialize();
            }

        }

        protected virtual void HandleImageData(string window_id, byte[] imageData)
        {
            IVRScreen VRScreen = Screens.list.Find(rect => rect.id == window_id);

            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imageData);

            bool update_placemnt = false;

            if (VRScreen == null)
            {
                // create a new rect and append
                VRScreen = new VRScreen(tex.width, tex.height, window_id,globalscale);
                Screens.list.Add(VRScreen);
                update_placemnt = true;
            }
            else if (VRScreen.widthInPixels != tex.width || VRScreen.heightInPixels != tex.height)
            {
                // update rect
                VRScreen.widthInPixels = tex.width;
                VRScreen.heightInPixels = tex.height;
                update_placemnt = true;
            }

            if (update_placemnt)
            {
                // update placement and generate screen
                VRScreenPlacer.RefreshPlacement();
                cylindricalMeshGenerator.RefreshScreens();
            }

            // update screen texture
            VRScreen.tex = tex;
            VRScreen.screen.GetComponent<MeshRenderer>().material.mainTexture = tex;
        }

        
        private void OnDestroy()
        {
            receiver.OnImageDataReceived -= HandleImageData;
        }
    }
}