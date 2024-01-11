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
    using System.Collections.Generic;
    using System.Diagnostics;
    using UnityEngine;

    ///< summary>
    /// Utility to create a curved mesh to draw screencast textures on.
    ///</summary>
    public class CylindricalMeshGenerator : MonoBehaviour
    {
        public ArcCylinderSpace _cylinderSpace;
        public VRScreens VRScreenComponent;
        public float divisionsPerDegree = 0.1f;
        public Material meshMaterial;
        private int _resolution;

        public void RefreshScreens()
        {

            foreach (var rect in VRScreenComponent.vrscreens)
            {

                Vector2[] corners = new Vector2[] { RemapNormalizedToArcSpace(rect.TopLeft()),
                                                    RemapNormalizedToArcSpace(rect.TopRight()),
                                                    RemapNormalizedToArcSpace(rect.BottomLeft()),
                                                    RemapNormalizedToArcSpace(rect.BottomRight()) };

                // Calculate resolution based on angular span and divisionsPerDegree
                float angularSpan =
                    Mathf.Abs(corners[1].x - corners[0].x);  // U values are in degrees
                _resolution = Mathf.CeilToInt(angularSpan * divisionsPerDegree);

                if (rect.screen != null)
                    DestroyImmediate(rect.screen);
                rect.screen = new GameObject("screen");
                rect.screen.transform.parent = this.transform;
                rect.screen.transform.localPosition = Vector3.zero;

                MeshFilter meshFilter = rect.screen.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = rect.screen.AddComponent<MeshRenderer>();

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                meshFilter.mesh = GenerateMesh(corners);
                meshRenderer.material = meshMaterial;  // Assign the material

                if (rect.Tex != null)
                {
                    // update placement screen texture
                    rect.screen.GetComponent<MeshRenderer>().material.mainTexture = rect.Tex;
                }

                stopwatch.Stop();
                long elapsedTicks = stopwatch.ElapsedTicks;
                double microseconds = (double)elapsedTicks / Stopwatch.Frequency * 1_000_000;
                UnityEngine.Debug.Log($"Time taken for mesh generation: {microseconds} �s");
            }
        }

        public Mesh GenerateMesh(Vector2[] corners)
        {
            return CylindricalMeshGeneratorStatic.GenerateMesh(corners, _cylinderSpace, _resolution,
                                                               0f);
        }

        // Method to remap the normalized corner values
        Vector2 RemapNormalizedToArcSpace(Vector2 normalizedCorner)
        {
            float u = Mathf.Lerp(-_cylinderSpace._arcAngle / 2, _cylinderSpace._arcAngle / 2,
                                 (normalizedCorner.x + 1) / 2);
            float v = Mathf.Lerp(0, _cylinderSpace._height, (normalizedCorner.y + 1) / 2);
            return new Vector2(u, v);
        }
    }

    // Static version of the CylindricalMeshGenerator's GenerateMesh method
    public static class CylindricalMeshGeneratorStatic
    {
        public static Mesh GenerateMesh(Vector2[] corners, ArcCylinderSpace cylinderSpace,
                                        float resolution, float offset)
        {
            Mesh mesh = new Mesh();

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            float uMin = corners[1].x;
            float uMax = corners[0].x;
            float vMin = corners[3].y;
            float vMax = corners[0].y;

            List<float> uPoints = GetInterpolatedPoints(corners[0].x, corners[1].x, resolution);
            float vTop = corners[0].y;
            float vBottom = corners[3].y;

            for (int i = 0; i < resolution; i++)
            {
                for (int j = 0; j < 2; j++)  // Only two points for v (top and bottom)
                {
                    float u = uPoints[i];
                    float v = (j == 0) ? vTop : vBottom;
                    Vector3 vertex =
                        cylinderSpace.ConvertFromAngleHeightToXYZWithOffset(u, v, offset);
                    vertices.Add(vertex);

                    float normalizedU = (u - uMin) / (uMax - uMin);
                    float normalizedV = (v - vMin) / (vMax - vMin);
                    uvs.Add(new Vector2(normalizedU, normalizedV));
                }
            }

            for (int i = 0; i < resolution - 1; i++)
            {
                int topLeft = i * 2;            // Top left vertex of a segment
                int topRight = topLeft + 2;     // Top right vertex of a segment
                int bottomLeft = topLeft + 1;   // Bottom left vertex of a segment
                int bottomRight = topLeft + 3;  // Bottom right vertex of a segment

                triangles.Add(topLeft);
                triangles.Add(bottomLeft);
                triangles.Add(topRight);

                triangles.Add(topRight);
                triangles.Add(bottomLeft);
                triangles.Add(bottomRight);
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();

            return mesh;
        }

        public static List<float> GetInterpolatedPoints(float _start, float _end, float _numPoints)
        {
            List<float> points = new List<float>();

            for (int i = 0; i < _numPoints; i++)
            {
                float t = (float)i / (_numPoints - 1);
                float point = Mathf.Lerp(_start, _end, t);
                points.Add(point);
            }

            return points;
        }
    }
}