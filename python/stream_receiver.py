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
"""Module which receives the applications from the windows machine."""

import queue
import socket
import struct
import threading
import cv2
import numpy as np
from window_display import WindowDisplay


class StreamReceiver:
  """Base class for the sharing client."""

  def __init__(self, host, port, slots=8):
    self.__host = host
    self.__port = port
    self.__slots = slots
    self._used_slots = 0
    self._running = False
    self.windows: dict[str, tuple[WindowDisplay, threading.Thread]] = {}
    self.updates = queue.Queue()
    self.interaction_events = queue.Queue()
    self.__block = threading.Lock()
    self.__server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    self.__init_socket()

  def __init_socket(self):
    self.__server_socket.bind((self.__host, self.__port))

  def start_server(self):
    """start the listening thread."""
    if self._running:
      print("Server is already running")
    else:
      self._running = True
      server_thread = threading.Thread(target=self.__server_listening)
      server_thread.start()

  def __server_listening(self):
    """start the listening."""
    self.__server_socket.listen()
    print(f"Server is listening on {self.__host}:{self.__port}")

    while self._running:
      self.__block.acquire()
      connection, _ = self.__server_socket.accept()
      if self._used_slots >= self.__slots:
        print("Connection refused! No free slots!")
        connection.close()
        self.__block.release()
        continue
      else:
        self._used_slots += 1
      self.__block.release()
      thread = threading.Thread(
          target=self.__client_connection, args=(connection,)
      )
      thread.start()

  def stop_server(self):
    """close the socket connection."""
    if self._running:
      self._running = False
      closing_connection = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
      closing_connection.connect((self.__host, self.__port))
      closing_connection.close()
      self.__block.acquire()
      self.__server_socket.close()
      self.__block.release()
    else:
      print("Server not running!")

  def __client_connection(self, connection):
    """generate two threads one for incomign data and one for outgoing data."""
    in_data_thread = threading.Thread(
        target=self.__handle_incoming_data, args=(connection,)
    )
    in_data_thread.start()

    out_data_thread = threading.Thread(
        target=self.__handle_out_data, args=(connection,)
    )
    out_data_thread.start()

  def __handle_incoming_data(self, connection):
    """handle incoming connection."""

    while self._running:
      try:
        # Receive size of metadata (window ID)
        metadata_size_struct = self.receive_all(
            connection, struct.calcsize("<L")
        )
        metadata_size = struct.unpack("<L", metadata_size_struct)[0]

        # Receive the actual metadata (window ID)
        metadata = connection.recv(metadata_size).decode("utf-8")
        window_id, data_type, expected_frame_size = metadata.split("|")
        expected_frame_size = int(expected_frame_size)

        data = b""

        while len(data) < expected_frame_size:
          data += connection.recv(4096)

        self._process_incoming_data(data, window_id, data_type)

      except UnicodeDecodeError:
        print("Received data is not valid UTF-8 encoded data.")
      except ValueError:
        print("Invalid metadata or frame size.")
      except ConnectionResetError as e:
        print(f"ConnectionResetError occurred: {e}")
        break
      except IncomingStreamingError as e:
        print(f"Exception occurred: {e}")
        connection.close()
        self._used_slots -= 1
        break

  def receive_all(self, sock, count):
    buf = b""
    while count:
      newbuf = sock.recv(count)
      if not newbuf:
        return None
      buf += newbuf
      count -= len(newbuf)
    return buf

  def _process_incoming_data(self, data, window_id, data_type):

    if data_type == "frame":
      frame = np.frombuffer(data, dtype=np.uint8)
      frame = cv2.imdecode(frame, cv2.IMREAD_COLOR)
      self.update_display_frame(window_id, frame)

  def __handle_out_data(self, connection):
    """handle out data."""

    while True:
      if not self.interaction_events.empty():
        event_to_send = self.interaction_events.get()
        print(event_to_send)
        bytes_to_send = event_to_send.to_bytes()
        packed_data_to_send_size = struct.pack("<L", len(bytes_to_send))
        combined_data = packed_data_to_send_size + bytes_to_send

        connection.sendall(combined_data)

        self.interaction_events.task_done()

  def update_display_frame(self, window_id, frame):
    """use the incoming data to update the displays."""

    displayer, _ = self.windows.get(window_id, (None, None))

    if displayer is None:
      displayer = WindowDisplay(
          frame, window_id, self.updates, self.interaction_events
      )
      image_thread = threading.Thread(target=displayer.display)

      self.windows[window_id] = (displayer, image_thread)
      image_thread.start()
    else:
      self.updates.put((displayer.updateimg, frame))

  def close_all_display(self):
    """close all the display objects."""

    for key in self.windows.items():
      self.updates.put((self.windows[key].quit, True))
      self.windows[key][1].join()

  def __del__(self):
    self.stop_server()
    self.close_all_display()


class IncomingStreamingError(Exception):
  """Custom exception for streaming server."""

  def __init__(
      self, message="the incoming streaming of serialized images had a problem"
  ):
    self.message = message
    super().__init__(message)


if __name__ == "__main__":
  # replace the ip with the ip of the machine you want to connect to

  server = StreamReceiver("127.0.0.1", 9999)
  server.start_server()

  while input("") != "STOP":
    continue

  server.stop_server()
