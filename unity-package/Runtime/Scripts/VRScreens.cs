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

namespace Google.XR.WindowShare
{
    using System.Collections.Generic;
    using UnityEngine;

    ///< summary>
    /// This class represent the VR screens (size in px, size in world
    /// coordinates,position, noormalized coordinates of the corners)
    /// ///</summary>
    public class VRScreens : MonoBehaviour
    {

        [System.Serializable]
        public class VRScreen
        {
            public string id;
            public string imageData;

            public float widthInPixels;
            public float heightInPixels;

            public GameObject _2DPlane;
            public GameObject screen;

            public Texture2D Tex;

            public float Width()
            {
                return _globalScale * widthInPixels;
            }

            public float Height()
            {
                return _globalScale * heightInPixels;
            }

            public float X;
            public float Y;

            // vector2(x,y) this functions
            // first normalize transform to [angle,height] by dividing the
            // value/VRScreenPlacer.hostingWidth (angle) and
            // value/VRScreenPlacer.hostingHeight then transform to [-1, 1] range
            // meaning the full range of the spatialcylinder
            // [-angle/2,+angle/2][-height/2,height/2] range (2*value-1) so to centere
            // in the cylindrical space finally asemble xs and ys in to corners

            public Vector2 BottomLeft()
            {
                return new Vector2(2 * (X / VRScreenPlacer.hostingWidth) - 1,
                                   2 * (Y / VRScreenPlacer.hostingHeight) - 1);
            }

            public Vector2 BottomRight()
            {
                return new Vector2(2 * ((X + Width()) / VRScreenPlacer.hostingWidth) - 1,
                                   2 * (Y / VRScreenPlacer.hostingHeight) - 1);
            }

            public Vector2 TopLeft()
            {
                return new Vector2(2 * (X / VRScreenPlacer.hostingWidth) - 1,
                                   2 * ((Y + Height()) / VRScreenPlacer.hostingHeight) - 1);
            }

            public Vector2 TopRight()
            {
                return new Vector2(2 * ((X + Width()) / VRScreenPlacer.hostingWidth) - 1,
                                   2 * ((Y + Height()) / VRScreenPlacer.hostingHeight) - 1);
            }

            public VRScreen(float widthPx, float heightPx, string window_id)
            {
                widthInPixels = widthPx;
                heightInPixels = heightPx;
                id = window_id;
            }

            public void Dispose()
            {
                // Destroy associated GameObjects
                if (_2DPlane != null)
                    Destroy(_2DPlane);

                if (screen != null)
                    Destroy(screen);
            }

            // OCR related stuff
            // public List<OCRRectangle> OCRRectangles = new List<OCRRectangle>();

            // Method to denormalize OCRRectangles from normal coordinates within
            // VRScreen to angular,height coordinates
            // public OCRRectangle NormalizeChildVRScreen(OCRRectangle ocrRect)
            // {

            //   // Adjust the OCRRectangle's position and size within the hosting
            //   // VRScreen
            //   float normalizedHPos = (X + ocrRect.ImgNormalizedHPos * Width()) /
            //                          VRScreenPlacer.hostingWidth;
            //   float normalizedVPos = (Y + ocrRect.ImgNormalizedVPos * Height()) /
            //                          VRScreenPlacer.hostingHeight;
            //   float normalizedWidth =
            //       ocrRect.ImgNormalizedWidth * Width() / VRScreenPlacer.hostingWidth;
            //   float normalizedHeight = ocrRect.ImgNormalizedHeight * Height() /
            //                            VRScreenPlacer.hostingHeight;

            //   // Transform to [-1, 1] range
            //   ocrRect.NormalizedBottomLeft =
            //       new Vector2(2 * normalizedHPos - 1, 2 * normalizedVPos - 1);
            //   ocrRect.NormalizedBottomRight = new Vector2(
            //       2 * (normalizedHPos + normalizedWidth) - 1, 2 * normalizedVPos -
            //       1);
            //   ocrRect.NormalizedTopLeft = new Vector2(
            //       2 * normalizedHPos - 1, 2 * (normalizedVPos + normalizedHeight) -
            //       1);
            //   ocrRect.NormalizedTopRight =
            //       new Vector2(2 * (normalizedHPos + normalizedWidth) - 1,
            //                   2 * (normalizedVPos + normalizedHeight) - 1);

            //   return ocrRect;
            // }

            // Method to normalize OCRRectangles within this VRScreen
            // public void CleanChildVRScreens()
            // {
            //   if (OCRRectangles.Count > 0)
            //   {
            //     foreach (OCRRectangle OCRRectangle in OCRRectangles)
            //     {
            //       if (OCRRectangle.OCRLineObject != null)
            //       {
            //         DestroyImmediate(OCRRectangle.OCRLineObject);
            //       }
            //     }
            //   }
            // }

            public Vector2 ConvertUVToPixel(Vector2 uv)
            {
                // First, denormalize the UV coordinates back to angle and height within
                // the VRScreen's space
                float angle = (uv.x + 1) / 2 * VRScreenPlacer.hostingWidth;
                float height = (uv.y + 1) / 2 * VRScreenPlacer.hostingHeight;

                // Next, calculate the pixel coordinates relative to the VRScreen
                float pixelX = widthInPixels - (angle - X) / _globalScale;
                float pixelY = heightInPixels - (height - Y) / _globalScale;

                return new Vector2(pixelX, pixelY);
            }

            // Method to check if a point (angle, height) is within any OCRRectangle
            // public OCRRectangle IsPointInsideAnyOCRRectangle(Vector2 point)
            // {
            //   foreach (var OCRrect in OCRRectangles)
            //   {
            //     if (IsPointInsideOCRRectangle(OCRrect, point))
            //     {
            //       return OCRrect; // Point is inside this VRScreen
            //     }
            //   }
            //   return null; // Point is not inside any VRScreen
            // }

            //   private bool IsPointInsideOCRRectangle(OCRRectangle rect, Vector2
            //   point)
            //   {
            //     // Check if point is within the VRScreen bounds
            //     Vector2 bottomLeft = rect.NormalizedBottomLeft;
            //     Vector2 topRight = rect.NormalizedTopRight;

            //     return point.x >= topRight.x && point.x <= bottomLeft.x &&
            //            point.y >= topRight.y && point.y <= bottomLeft.y;
            //   }
        }

        private static float _globalScale = 0.0008f;

        [SerializeField]
        private float editorGlobalScale = 0.0008f;  // This will be visible in the editor
        public float EditorGlobalScale
        {
            get {
                return _globalScale;
            }
            set {
                _globalScale = value;
                editorGlobalScale = value;
            }
        }

        void OnValidate()  // This method is called whenever a value is changed in the
                           // editor
        {
            _globalScale = editorGlobalScale;
        }

        public static float GlobalScale
        {
            get {
                return _globalScale;
            }
        }

        [SerializeField]
        private List<VRScreen> _VRScreens = new List<VRScreen>();
        public List<VRScreen> vrscreens
        {
            get {
                return _VRScreens;
            }
            set {
                // Call Dispose on any VRScreens that are in _VRScreens but not in value
                foreach (var existingRect in _VRScreens)
                {
                    if (!value.Contains(existingRect))
                    {
                        existingRect.Dispose();
                    }
                }
                _VRScreens = value;
            }
        }

        // Method to check if a point (angle, height) is within any VRScreen
        public VRScreen IsPointInsideAnyVRScreen(Vector2 point)
        {
            foreach (var VRScreen in _VRScreens)
            {
                if (IsPointInsideVRScreen(VRScreen, point))
                {
                    return VRScreen;  // Point is inside this VRScreen
                }
            }
            return null;  // Point is not inside any VRScreen
        }

        private bool IsPointInsideVRScreen(VRScreen rect, Vector2 point)
        {
            // Check if point is within the VRScreen bounds
            Vector2 bottomLeft = rect.BottomLeft();
            Vector2 topRight = rect.TopRight();

            return point.x >= bottomLeft.x && point.x <= topRight.x && point.y >= bottomLeft.y &&
                   point.y <= topRight.y;
        }
    }
}