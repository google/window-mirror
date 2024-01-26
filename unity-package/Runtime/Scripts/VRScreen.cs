// <copyright file="VRScreen.cs" company="Google LLC">
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
    using UnityEngine;

    ///<summary>
    /// This class represent a VR screens (size in px, size in world
    /// coordinates,position, normalized coordinates of the corners)
    ///</summary>

    [System.Serializable]
    public class VRScreen : IVRScreen
    {
        public string id;
        string IVRScreen.id
        {
            get
            {
                return id;
            }
        }
        public float widthInPixels;
        float IVRScreen.widthInPixels
        {
            get
            {
                return widthInPixels;
            }
            set
            {
                widthInPixels = value;
            }
        }
        public float heightInPixels;
        float IVRScreen.heightInPixels
        {
            get
            {
                return heightInPixels;
            }
            set
            {
                heightInPixels = value;
            }
        }
        public float X;
        float IVRScreen.X
        {
            get
            {
                return X;
            }
            set
            {
                X = value;
            }
        }
        public float Y;
        float IVRScreen.Y
        {
            get
            {
                return Y;
            }
            set
            {
                Y = value;
            }
        }
        public GameObject plane2D;
        GameObject IVRScreen.plane2D
        {
            get
            {
                return plane2D;
            }
            set
            {
                plane2D = value;
            }
        }
        public GameObject screen;
        GameObject IVRScreen.screen
        {
            get
            {
                return screen;
            }
            set
            {
                screen = value;
            }
        }
        public Texture2D tex;
        Texture2D IVRScreen.tex
        {
            get
            {
                return tex;
            }
            set
            {
                tex = value;
            }
        }

        public float _globalScale;

        public byte[] textureData;
        byte[] IVRScreen.textureData
        {
            get
            {
                return textureData;
            }
            set
            {
                textureData = value;
            }
        }

        public VRScreen(float widthPx, float heightPx, string window_id, float globalscale)
        {
            widthInPixels = widthPx;
            heightInPixels = heightPx;
            id = window_id;
            _globalScale = globalscale;
        }

        public float Width()
        {
            return _globalScale * widthInPixels;
        }

        public float Height()
        {
            return _globalScale * heightInPixels;
        }

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
    }

    ///<summary>
    /// This class represent a VR screens abstraction
    ///</summary>
    public interface IVRScreen
    {
        public string id { get; }
        public float widthInPixels { get; set; }
        public float heightInPixels { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public GameObject plane2D { get; set; }
        public GameObject screen { get; set; }
        public Texture2D tex { get; set; }
        public byte[] textureData { get; set; }

        float Width();
        float Height();
        Vector2 BottomLeft();
        Vector2 BottomRight();
        Vector2 TopLeft();
        Vector2 TopRight();
        Vector2 ConvertUVToPixel(Vector2 uv);
    }
}