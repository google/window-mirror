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
    using System.Linq;
    using UnityEngine;

    ///< summary>
    /// This class spatially organizes VRScreens on a 2D layout.
    ///</summary>
    public class VRScreenPlacer : MonoBehaviour
    {
        public VRScreens VRScreensContainer;

        [Header("Toggles to show screens")]
        public bool displayVRScreens = true;
        public bool displayHostingVRScreen = true;

        [Header("Space size")]
        public static float hostingWidth;
        public static float hostingHeight;

        [Header("Arc Cylinder Space Integration")]
        public bool useArcCylinderSpaceDimensions = false;
        public ArcCylinderSpace arcCylinderSpace;

        [Header("Spacing and offsets")]
        public float spacing = 0.2f;
        public float offset = -5.0f;

        private GameObject hostingplane;

        private float _averageRowWidth;
        private List<List<VRScreens.VRScreen>> _bestDistribution = null;
        private float _bestScore = float.MaxValue;

        public void RefreshPlacement()
        {
            // Update hosting dimensions if using ArcCylinderSpace dimensions
            if (useArcCylinderSpaceDimensions && arcCylinderSpace)
            {
                Vector2 arcDimensions = arcCylinderSpace.GetVRScreenDimensions();
                hostingWidth = arcDimensions.x;
                hostingHeight = arcDimensions.y;
            }

            // Delete all recorded planes
            foreach (var vrs in VRScreensContainer.vrscreens)
            {
                if (vrs._2DPlane != null)
                    DestroyImmediate(vrs._2DPlane);
            }
            if (hostingplane != null)
                DestroyImmediate(hostingplane);

            DistributeAndPlaceVRScreens(VRScreensContainer.vrscreens, hostingWidth);
            if (displayHostingVRScreen)
            {
                DisplayHostingVRScreen();
            }
            if (displayVRScreens)
            {
                DisplayVRScreens();
            }
        }

        public void DistributeAndPlaceVRScreens(List<VRScreens.VRScreen> rects, float hostingWidth)
        {
            // 1. Calculate total width of all VRScreens
            float totalWidth = rects.Sum(r => r.Width());
            _averageRowWidth = totalWidth / Mathf.CeilToInt(totalWidth / hostingWidth);

            // 2. Recursively try to form rows
            _bestDistribution = null;
            _bestScore = float.MaxValue;
            TryFormRows(new List<VRScreens.VRScreen>(rects), new List<List<VRScreens.VRScreen>>());

            // 3. Place the VRScreens using the best distribution
            float totalRowsHeight = CalculateTotalRowsHeight(_bestDistribution);
            int rowIndex = 0;
            foreach (var row in _bestDistribution)
            {
                PlaceRow(row, hostingWidth, hostingHeight, totalRowsHeight, rowIndex);
                rowIndex++;
            }
        }

        private void TryFormRows(List<VRScreens.VRScreen> remainingRects,
                                 List<List<VRScreens.VRScreen>> currentDistribution)
        {
            if (remainingRects.Count == 0)
            {
                float currentScore = CalculateScore(currentDistribution);
                if (currentScore < _bestScore)
                {
                    _bestScore = currentScore;
                    _bestDistribution = new List<List<VRScreens.VRScreen>>(currentDistribution);
                }
                return;
            }

            for (int i = 1; i <= remainingRects.Count; i++)
            {
                List<VRScreens.VRScreen> row = remainingRects.Take(i).ToList();
                if (row.Sum(r => r.Width()) <=
                    _averageRowWidth * 1.5)  // Allowing rows to be up to 50% longer than average
                {
                    currentDistribution.Add(row);
                    TryFormRows(remainingRects.Skip(i).ToList(), currentDistribution);
                    currentDistribution.RemoveAt(currentDistribution.Count - 1);
                }
            }
        }

        private float CalculateScore(List<List<VRScreens.VRScreen>> distribution)
        {
            float deviationScore =
                distribution.Sum(row => Mathf.Abs(row.Sum(r => r.Width()) - _averageRowWidth));
            float rowCountScore = distribution.Count;
            return deviationScore + rowCountScore * 1000;  // Weighting row count more heavily
        }

        private void PlaceRow(List<VRScreens.VRScreen> row, float hostingWidth, float hostingHeight,
                              float totalRowsHeight, int rowIndex)
        {
            float totalRowWidth = row.Sum(r => r.Width());
            float currentX = (hostingWidth - totalRowWidth) / 2;

            float rowHeight = row.Max(r => r.Height());

            // Calculate the starting Y position such that the rows are centered in
            // hostingHeight
            float startY = (hostingHeight - totalRowsHeight) / 2;
            float accumulatedHeight = rowIndex * rowHeight +
                                      Mathf.Min(rowIndex, row.Count - 1) *
                                          spacing;  // add spacing only for rows before the last row
            float currentY = startY + accumulatedHeight;

            foreach (var rect in row)
            {
                rect.X = currentX;

                // Adjust the Y position for vertical centering within the row
                float verticalOffset = (rowHeight - rect.Height()) / 2;
                rect.Y = currentY + verticalOffset;

                currentX += rect.Width() + spacing;
            }
        }

        float CalculateTotalRowsHeight(List<List<VRScreens.VRScreen>> allRows)
        {
            float totalRowsHeight = 0;
            for (int i = 0; i < allRows.Count; i++)
            {
                float rowHeight = allRows[i].Max(r => r.Height());
                totalRowsHeight += rowHeight;

                // Only add spacing if it's not the last row
                if (i < allRows.Count - 1)
                    totalRowsHeight += spacing;
            }
            return totalRowsHeight;
        }

        private void DisplayVRScreens()
        {
            foreach (var r in VRScreensContainer.vrscreens)
            {
                var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.transform.parent = transform;
                plane.transform.localScale = new Vector3(r.Width() * 0.1f, 1, r.Height() * 0.1f);
                plane.transform.localPosition =
                    new Vector3(r.X + r.Width() * 0.5f - hostingWidth * 0.5f, offset,
                                hostingHeight * 0.5f - (r.Y + r.Height() * 0.5f));
                r._2DPlane = plane;
            }
        }

        private void DisplayHostingVRScreen()
        {
            hostingplane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            hostingplane.transform.parent = transform;
            hostingplane.transform.localScale =
                new Vector3(hostingWidth * 0.1f, 1, hostingHeight * 0.1f);
            hostingplane.transform.localPosition =
                new Vector3(0, offset - 0.01f, 0);  // Set Y to -0.01
            var renderer = hostingplane.GetComponent<Renderer>();
            renderer.material.color = Color.gray;
        }
    }
}