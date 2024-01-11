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
    using UnityEngine;
    using System;

    ///< summary>
    /// This class queues events from asyncronous threads (i.e. websocket texture
    /// serializations) and dispatches them onto the main thread (i.e. thread where
    /// unity updates geometry and textures).
    ///</summary>
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static readonly Queue<Action> ExecuteOnMainThreadQueue = new Queue<Action>();
        private static readonly List<Action> ExecuteCopiedQueue = new List<Action>();

        private void Update()
        {
            // Lock the queue and transfer actions to the copied queue
            lock (ExecuteOnMainThreadQueue)
            {
                while (ExecuteOnMainThreadQueue.Count > 0)
                {
                    ExecuteCopiedQueue.Add(ExecuteOnMainThreadQueue.Dequeue());
                }
            }

            // Execute the copied actions
            foreach (var action in ExecuteCopiedQueue)
            {
                action.Invoke();
            }

            // Clear the action list
            ExecuteCopiedQueue.Clear();
        }

        public static void ExecuteOnMainThread(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            lock (ExecuteOnMainThreadQueue)
            {
                ExecuteOnMainThreadQueue.Enqueue(action);
            }
        }
    }
}