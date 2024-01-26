// <copyright file="VRScreenList.cs" company="Google LLC">
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
    using System.Linq;

    using UnityEngine;

    ///<summary>
    /// This scriptable object enable creating a debug list of VR Screens.
    ///</summary>
    [CreateAssetMenu(fileName = "VRScreenList", menuName = "ScriptableObjects/VRScreenList", order = 1)]
    public class VRScreenList : ScriptableObject, IVRScreenList
    {
        public List<VRScreen> vrScreens = new List<VRScreen>();

        public List<IVRScreen> GetScreens()
        {
            return vrScreens.Cast<IVRScreen>().ToList();
        }

        public void SetScreens(List<IVRScreen> screens)
        {
            vrScreens = screens.Cast<VRScreen>().ToList();
        }

        public void ClearScreens()
        {
            vrScreens.Clear();
        }
    }

    public interface IVRScreenList
    {
        List<IVRScreen> GetScreens();
        void SetScreens(List<IVRScreen> screens);
        void ClearScreens();
    }
}