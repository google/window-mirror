# Copyright 2024 Google LLC
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     https://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
"""Module which streams the applications from the windows machine."""

import hashlib
import queue
import socket
import struct
import threading
import time
from typing import List

import cv2
from events import UIevent
from window_capture import ScreenCaptureError
from window_capture import WindowCapture
from window_selection import WindowSelection
from window_simulated_interaction import InteractionSimulator


class StreamingClient:
  """Handles the streaming of window captures."""

  def __init__(self, window_title, window_hwd, shared_connection):
    """Initializes the streaming client with window and connection details."""
    self.window_title = window_title
    self.shared_connection = shared_connection
    self.window_id = window_hwd
    self.fps = 5
    self.frame_time = 1.0 / self.fps
    self.new_frame_avaliable = False
    self._frame_changed = True
    self.prev_frame_hash = None
    self.window = WindowCapture(self.window_title)

    self.stop_stream_event = queue.Queue()
    self.client_thread = None
    self._running = False
    self._configure()

  def _configure(self):
    """Configures encoding parameters for streaming."""
    self.__encoding_parameters = [int(cv2.IMWRITE_JPEG_QUALITY), 80]

  def _get_frame(self):
    """Captures a single frame from the specified window.

    Returns:
        frame (numpy.ndarray): Captured frame from the window.
    """
    try:
      frame = self.window.screenshot()
      frame = cv2.cvtColor(frame, cv2.COLOR_RGBA2RGB)
      return frame
    except ScreenCaptureError as e:
      print("An unexpected error occured " + str(e))
      return None

  def __client_streaming(self):
    """Internal method to handle streaming framerate."""
    while self._running:
      start_time = time.time()

      # Get frame from window capturing module
      frame = self._get_frame()

      if frame is None:
        continue

      self._process_frame(frame)

      # Introduce delay to achieve the desired fps
      elapsed_time = time.time() - start_time
      if elapsed_time < self.frame_time:
        time.sleep(self.frame_time - elapsed_time)

  def _process_frame(self, frame):
    """Processes each captured frame and sends it if there are changes.

    Args:
        frame (numpy.ndarray): The current frame to be processed.
    """

    # check if a new frame is avaliable
    self._frame_changed = self.__has_frame_changed(frame)

    if self._frame_changed:
      _, frame = cv2.imencode(".jpg", frame, self.__encoding_parameters)

      try:
        self.shared_connection.send_data(self.window_id, frame.tobytes(),
                                         "frame")
      except ConnectionResetError:
        self._running = False
      except ConnectionAbortedError:
        self._running = False
      except BrokenPipeError:
        self._running = False

  def __has_frame_changed(self, frame):
    """Checks if the current frame is different from the previous one.

    Args:
        frame: The current frame to be checked.

    Returns:
        bool: True if the frame has changed, False otherwise.
    """
    current_hash = hashlib.md5(frame.tobytes()).hexdigest()

    if self.prev_frame_hash is None:
      self.prev_frame_hash = current_hash
      return True

    has_changed = current_hash != self.prev_frame_hash
    if has_changed:
      self.prev_frame_hash = current_hash

    return has_changed

  def start_stream(self):
    """Method to start the stream."""
    if self._running:
      print("Client is already streaming!")
    else:
      self._running = True
      self.client_thread = threading.Thread(target=self.__client_streaming)
      self.client_thread.start()

  def stop_stream(self):
    """Method to stop the stream."""
    if self._running:
      self._running = False
      self.stop_stream_event.put(("stop_stream", self.window_id))
    else:
      print("Client not streaming!")


class SharedConnectionClient:
  """Base class that implement connection."""

  def __init__(self, host, port):
    """Method to initialize the class.

    Args:
      host: ip that the msule will connect to.
      port: port the module will connect to.
    """
    self._host = host
    self._port = port
    self.interaction_queue = queue.Queue()
    self.interaction_simulator = InteractionSimulator(self.interaction_queue)
    self.interaction_simulator_thread = threading.Thread(
        target=self.interaction_simulator.process_queue)
    self.interaction_simulator_thread.start()
    self.receive_data_thread = None
    self._client_socket = None
    self._connect()

  def _connect(self, max_attempts=5, delay=2):
    """Handles connection and reconnection attempts."""
    if self._client_socket is not None:
      self._client_socket.close()
    self._client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    attempts = 0
    while attempts < max_attempts:
      try:
        self._client_socket.connect((self._host, self._port))
        print("Connection successful.")
        self._restart_receive_data_thread()
        return
      except socket.error as e:
        print(f"Connection attempt {attempts + 1} failed: {e}")
        time.sleep(delay)
        attempts += 1
    raise ConnectionError("Failed to connect after several attempts.")

  def _restart_receive_data_thread(self):
    """Restarts the data receiving thread."""
    if (self.receive_data_thread is not None and
        self.receive_data_thread.is_alive()):
      pass
    self.receive_data_thread = threading.Thread(target=self.__receive_data)
    self.receive_data_thread.start()

  def send_data(self, window_id, data: bytes, data_type):
    """Method to send data.

    Args:
      window_id: identifier of the window the data sent belongs to.
      data: the serialized data to be sent.
      data_type: ui event type being sent.
    """
    try:
      size = len(data)
      metadata = f"{window_id}|{data_type}|{size}".encode()

      packed_metadata_size = struct.pack("<L", len(metadata))

      combined_data = packed_metadata_size + metadata + data
      self._client_socket.sendall(combined_data)
    except OSError as e:
      print(f"An OSError occurred: {e}")

  def __receive_data(self):
    """Method to receive data."""

    while True:
      try:
        size_struct = self._client_socket.recv(struct.calcsize("<L"))
        data_size = struct.unpack("<L", size_struct)[0]
        data = self._client_socket.recv(data_size)
        received_event = self._data_to_event(data)
        self.interaction_queue.put(received_event)

      except UnicodeDecodeError:
        print("Received data is not valid UTF-8 encoded data.")
      except IncomingStreamingError as e:
        print(f"Exception occurred: {e}")
        self._client_socket.close()
      except (ConnectionResetError, socket.error) as e:
        print(f"Connection error occurred: {e}")
        self._connect()

  def _data_to_event(self, data):
    """Method to close the connection."""
    received_event = UIevent()
    received_event.from_bytes(data)
    return received_event

  def close(self):
    """Method to close the connection."""
    self._client_socket.close()


class IncomingStreamingError(Exception):
  """Custom exception for streaming server."""

  def __init__(
      self,
      message="the incoming streaming of serialized images had a problem"):
    self.message = message
    super().__init__(message)


if __name__ == "__main__":
  # replace the ip with the ip of the machine you want to connect to

  ip = "127.0.0.1"

  connection = SharedConnectionClient(ip, 9999)
  WindowSelector = WindowSelection()

  streaming_clients: List[StreamingClient] = []

  for i in range(3):
    title, hWnd = WindowSelector.select()
    print(f"Selected window {i+1}: {title}")
    window_client = StreamingClient(title, hWnd, connection)
    window_client.start_stream()
    streaming_clients.append(window_client)
    print("Window sharing has begun. Use ctrl-C to stop.")

  while input("") != "STOP":
    continue

  for client in streaming_clients:
    client.stop_stream()
