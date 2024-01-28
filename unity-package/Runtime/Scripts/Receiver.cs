// <copyright file="Receiver.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using UnityEngine;
    using TMPro;

    ///<summary>
    /// This class receives the serialized image data via websokets.
    ///</summary>
    public class Receiver : MonoBehaviour
    {
        public string ipAddress = "127.0.0.1";
        public int port = 9999;
        public TextMeshPro text;
        public Queue<UIEvent> uIEventsQueue = new Queue<UIEvent>();
        public event Action<string, byte[]> OnImageDataReceived;
        public MainThreadDispatcher mainThreadDispatcher;

        private TcpListener _listener;
        protected TcpClient _client;

        void Start()
        {

#if UNITY_EDITOR
            ipAddress = "127.0.0.1";
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
            ipAddress = GetLocalIPv4();
#endif

            _listener = new TcpListener(IPAddress.Parse(ipAddress), port);
            _listener.Start();
            _listener.BeginAcceptTcpClient(HandleClient, null);
        }

        public string GetLocalIPv4()
        {
            string address = "";

            address =
                Dns.GetHostEntry(Dns.GetHostName())
                    .AddressList
                    .First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .ToString();

            Debug.Log(address);

            if (text != null)
            {

                text.text = address;
            }

            return address;
        }

        void HandleClient(IAsyncResult ar)
        {
            _client = _listener.EndAcceptTcpClient(ar);

            Thread streamReader = new Thread(new ThreadStart(StreamReader));

            streamReader.Start();

            Thread uiEventSender = new Thread(new ThreadStart(UiEventSender));

            uiEventSender.Start();
        }

        private void StreamReader()
        {
            NetworkStream stream = _client.GetStream();

            while (true)
            {
                // Read the size of the metadata
                byte[] metadataSizeBuffer = new byte[4];
                int bytesRead = stream.Read(metadataSizeBuffer, 0, metadataSizeBuffer.Length);
                if (bytesRead != 4)
                {
                    // Handle this error scenario (maybe the connection was closed or there
                    // was a transmission error)
                    break;
                }

                int metadataSize = BitConverter.ToInt32(metadataSizeBuffer, 0);

                // Now, read the actual metadata
                byte[] metadataBuffer = new byte[metadataSize];
                bytesRead = stream.Read(metadataBuffer, 0, metadataSize);
                if (bytesRead != metadataSize)
                {
                    // Handle this error scenario
                    break;
                }
                string metadata = System.Text.Encoding.UTF8.GetString(metadataBuffer);
                string[] parts = metadata.Split('|');
                if (parts.Length != 3)
                {
                    break;
                }

                string window_id = parts[0];
                string data_type = parts[1];
                int dataSize = Convert.ToInt32(parts[2]);

                // Now, read the actual image data
                byte[] data = new byte[dataSize];
                int totalBytesRead = 0;
                while (totalBytesRead < dataSize)
                {
                    bytesRead = stream.Read(data, totalBytesRead, dataSize - totalBytesRead);
                    if (bytesRead == 0)
                    {
                        // The connection was probably closed
                        break;
                    }
                    totalBytesRead += bytesRead;
                }

                // Emit the event with window id and image data, but dispatch it to the
                // main thread
                EmitEvent(data_type, window_id, data);
            }
        }

        protected virtual void EmitEvent(string data_type, string window_id, byte[] data)
        {

            // Emit the event with window id and image data, but dispatch it to the
            // main thread
            MainThreadDispatcher.ExecuteOnMainThread(() =>
                                                     {
                                                         if (data_type == "frame")
                                                         {
                                                             RaiseOnImageDataReceived(window_id, data);
                                                         }
                                                     });
        }

        protected void RaiseOnImageDataReceived(string window_id, byte[] data)
        {
            OnImageDataReceived?.Invoke(window_id, data);
        }

        private void UiEventSender()
        {
            UIEvent e;

            try
            {
                while (true)
                {
                    if (uIEventsQueue.Count > 0)
                    {

                        e = uIEventsQueue.Dequeue();

                        if (e == null)
                        {
                            continue;
                        }

                        byte[] message = e.ToBytes();
                        byte[] size =
                            BitConverter.GetBytes((uint)message.Length);  // Use uint for 4 bytes

                        // Ensure little-endian format if system is big-endian
                        if (!BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(size);
                        }

                        IEnumerable<byte> combined = size.Concat(message);
                        _client.Client.Send(combined.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error in UiEventSender" + ex.Message);
            }
        }

        private void OnApplicationQuit()
        {
            _listener.Stop();
        }
    }
}