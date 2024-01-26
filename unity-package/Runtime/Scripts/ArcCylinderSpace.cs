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

namespace Google.XR.WindowMirror
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    ///<summary>
    /// Utility to map between XYZ and coordinates on a curved screencast surface.
    ///</summary>
    public class ArcCylinderSpace : MonoBehaviour
    {
        public float _radius = 3;
        public float _height = 8;
        public float _arcAngle = 160;  // Add arc angle as a class property

        public ArcCylinderSpace(float radius, float height, float arcAngle)
        {
            _radius = radius;
            _height = height;
            _arcAngle = arcAngle;  // Store the arc angle
        }

        public Vector3 ConvertFromNormalizedUVToXYZ(float u, float v)
        {
            // Remap u from [-1, 1] to [-arcAngle/2, arcAngle/2]
            u = Mathf.Lerp(-_arcAngle / 2, _arcAngle / 2, (u + 1) / 2);

            // Remap v from [-1, 1] to [0, height]
            v = Mathf.Lerp(0, _height, (v + 1) / 2);

            float x = _radius * Mathf.Cos(Mathf.Deg2Rad * u);
            float y = v - (_height / 2);  // centering v around 0
            float z = _radius * Mathf.Sin(Mathf.Deg2Rad * u);

            return new Vector3(x, y, z);
        }

        public Vector3 ConvertFromAngleHeightToXYZ(float u, float v)
        {
            return ConvertFromAngleHeightToXYZWithOffset(u, v, 0f);
        }

        public Vector3 ConvertFromAngleHeightToXYZWithOffset(float u, float v, float offset)
        {
            // Add offset to the radius
            float effectiveRadius = _radius + offset;

            // u will be mapped from [-arcAngle/2, arcAngle/2] in degrees
            // v will be mapped from [0, height]

            float x = effectiveRadius * Mathf.Cos(Mathf.Deg2Rad * u);
            float y = v - (_height / 2);  // centering v around 0
            float z = effectiveRadius * Mathf.Sin(Mathf.Deg2Rad * u);

            return new Vector3(x, y, z);
        }

        // Function to get the VRScreen dimensions
        public Vector2 GetVRScreenDimensions()
        {
            float width = Mathf.Abs(_arcAngle * Mathf.Deg2Rad * _radius);  // Arc length
            float height = _height;
            return new Vector2(width, height);
        }

        public Vector2 ConvertFromNormalizedUVToAngleHeight(float u, float v)
        {
            // Convert normalized u (from [-1, 1]) back to angle (from [-arcAngle/2,
            // arcAngle/2])
            float angle = Mathf.Lerp(-_arcAngle / 2, _arcAngle / 2, (u + 1) / 2);

            // Convert normalized v (from [-1, 1]) back to height (from [0, height])
            float heightPosition = Mathf.Lerp(0, _height, (v + 1) / 2);

            return new Vector2(angle, heightPosition);
        }

        public Vector2 ConvertXYZToUV(Vector3 worldPosition)
        {
            // Calculate the angle based on worldPosition
            float angle = Mathf.Atan2(worldPosition.z, worldPosition.x) * Mathf.Rad2Deg;

            // Normalize angle from [-arcAngle/2, arcAngle/2] to [-1, 1]
            float normalizedAngle = (angle + _arcAngle / 2) / (_arcAngle / 2) - 1;

            // Normalize height from [0, height] to [-1, 1]
            // Assuming worldPosition.y is already in the range [0, _height]
            float normalizedHeight = (worldPosition.y / _height) * 2;

            return new Vector2(normalizedAngle, normalizedHeight);
        }
    }

}