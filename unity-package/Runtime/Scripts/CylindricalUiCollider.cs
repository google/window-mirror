// <copyright file="CylindricalUiCollider.cs" company="Google LLC">
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
    using System;
    using TMPro;
    using UnityEngine;

    ///<summary>
    /// Utility to create a curved mesh representing the whole Cylindrical space to
    /// capture ui events using System.
    ///</summary>
    public class CylindricalUiCollider : MonoBehaviour
    {
        protected GameObject uiCylinderSpace;

        public Receiver receiver;
        public ArcCylinderSpace cylinderSpace;
        public VRScreens screens;
        public float offset = -0.01f;  // Offset for the mesh
        public float divisionsPerDegree = 0.0f;
        public Material meshMaterial;
        public bool render;
        public TextMeshPro textcomponent;
        public bool displaytextbool;

        public void Initialize()
        {
            // generate gameobject
            uiCylinderSpace = new GameObject("uiCylinderSpace");
            uiCylinderSpace.transform.parent = gameObject.transform;

            // Add a MeshFilter and generate mesh
            MeshFilter meshFilter = uiCylinderSpace.AddComponent<MeshFilter>();
            meshFilter.mesh = GenerateColliderMesh();

            if (render)
            {

                MeshRenderer meshRenderer = uiCylinderSpace.AddComponent<MeshRenderer>();
                meshRenderer.material = meshMaterial;
            }

            // Add a MeshCollider component
            MeshCollider meshCollider = uiCylinderSpace.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilter.mesh;
            meshCollider.enabled = true;
        }

        private Mesh GenerateColliderMesh()
        {
            // Define the corners of the cylindrical space in normalized coordinates
            Vector2[] corners =
                new Vector2[] { cylinderSpace.ConvertFromNormalizedUVToAngleHeight(-1, 1),
                                cylinderSpace.ConvertFromNormalizedUVToAngleHeight(1, 1),
                                cylinderSpace.ConvertFromNormalizedUVToAngleHeight(1, -1),
                                cylinderSpace.ConvertFromNormalizedUVToAngleHeight(-1, -1) };

            // Calculate resolution based on angular span and divisionsPerDegree
            float angularSpan = Mathf.Abs(corners[1].x - corners[0].x);  // U values are in degrees
            var _resolution = Mathf.CeilToInt(angularSpan * divisionsPerDegree);

            // Use the mesh generation logic from CylindricalMeshGenerator
            return CylindricalMeshGeneratorStatic.GenerateMesh(corners, cylinderSpace, _resolution,
                                                               offset);
        }

        public void HandleHit(Vector3 point, UIEventsTypes eventType, int value = 0)
        {
            value = (int)value * 10;
            // Convert hit point to AngleHeight coordinates
            Vector2 uv = cylinderSpace.ConvertXYZToUV(point);

            // check if is inside of a VRScreen
            var rect = screens.IsPointInsideAnyVRScreen(uv);

            if (rect != null)
            {
                var px = rect.ConvertUVToPixel(uv);
                Debug.Log(
                    $"Event Triggered: {eventType}, Value :{value}, UV: {uv}, XYZ:{point}, px:{px}");
                UIEvent uie =
                    new UIEvent(eventType, value, (int)px.x, (int)px.y, int.Parse(rect.id));
                receiver.uIEventsQueue.Enqueue(uie);
            }
            else
            {
                // Handle the hit based on the eventType and UV coordinates
                Debug.Log($"Event Triggered: {eventType}, UV: {uv}, XYZ:{point}");
            }
        }

        public void HandleKeyPress(UIEventsTypes eventType, int ascii_keyCode)
        {
            Debug.Log($"Event Triggered: {eventType}, Value :{ascii_keyCode}");
            UIEvent uie = new UIEvent(eventType, ascii_keyCode);
            receiver.uIEventsQueue.Enqueue(uie);
        }
    }
}