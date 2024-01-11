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
"""Module providing a interaction simulation."""

import queue
from events import ClickType
from events import UIevent
import pyautogui
import win32con
import win32gui
from window_selection import WindowSelection


class InteractionSimulator:
  """Class providing a interaction simulation."""

  def __init__(self, interaction_queue=queue.Queue()):
    self.interaction_queue = interaction_queue
    pyautogui.PAUSE = 0.0

  def bring_to_foreground(self, hwnd):
    """Bring the window to foreground."""

    if not win32gui.IsWindowVisible(hwnd):
      raise WindowNotVisibleError()
    if win32gui.GetForegroundWindow() != hwnd:
      win32gui.ShowWindow(hwnd, win32con.SW_RESTORE)
      win32gui.SetWindowPos(
          hwnd,
          win32con.HWND_TOPMOST,
          0,
          0,
          0,
          0,
          win32con.SWP_NOMOVE | win32con.SWP_NOSIZE,
      )
      win32gui.SetWindowPos(
          hwnd,
          win32con.HWND_NOTOPMOST,
          0,
          0,
          0,
          0,
          win32con.SWP_NOMOVE | win32con.SWP_NOSIZE,
      )

  def convert_to_global_coordinates(self, hwnd, local_x, local_y):
    """Convert local window to global screen coordinates."""
    left, top, _, _ = win32gui.GetWindowRect(hwnd)
    global_x = left + local_x
    global_y = top + local_y
    return global_x, global_y

  def mouse_click(self, hwnd, local_x, local_y, button):
    """Simulate single click."""
    global_x, global_y = self.convert_to_global_coordinates(
        hwnd, local_x, local_y
    )
    pyautogui.click(x=global_x, y=global_y, button=button)

  def mouse_double_click(self, hwnd, local_x, local_y, button):
    """Simulate double click."""
    global_x, global_y = self.convert_to_global_coordinates(
        hwnd, local_x, local_y
    )
    pyautogui.doubleClick(x=global_x, y=global_y, button=button)

  def mouse_scroll(self, hwnd, local_x, local_y, scrollvalue):
    """Simulate scroll click."""
    global_x, global_y = self.convert_to_global_coordinates(
        hwnd, local_x, local_y
    )
    pyautogui.moveTo(x=global_x, y=global_y)
    pyautogui.scroll(int(scrollvalue), x=global_x, y=global_y)

  def key_press(self, ascii_value):
    """Simulate key press."""

    if ascii_value == 13:
      pyautogui.press("enter")
    elif ascii_value == 255:
      pyautogui.press("left")
    elif ascii_value == 254:
      pyautogui.press("right")
    elif ascii_value == 253:
      pyautogui.press("up")
    elif ascii_value == 252:
      pyautogui.press("down")
    char_value = chr(ascii_value)
    pyautogui.press(char_value)

  def write(self, text):
    """Simulate writing text."""
    pyautogui.write(text, interval=0.1)

  def simulate_interaction_event(self, event: UIevent):
    """Handle event to be Simulated."""
    if event.is_valid():
      task = event.event_task()
      if task == ClickType.KEY_PRESS:
        print("I shall perform a key_press")
        self.key_press(event.value)
      else:
        self.bring_to_foreground(event.window_id)
        if task == ClickType.MOUSE_CLICK:
          print("I shall perform a mouse_click")
          self.mouse_click(
              event.window_id, event.x, event.y, event.button().name.lower()
          )
        elif task == ClickType.MOUSE_DOUBLE_CLICK:
          print("I shall perform a mouse_double_click")
          self.mouse_double_click(
              event.window_id, event.x, event.y, event.button().name.lower()
          )
        elif task == ClickType.MOUSE_SCROLL:
          print("I shall perform a mouse_scroll")
          self.mouse_scroll(event.window_id, event.x, event.y, event.value)

  def process_queue(self):
    """wait for the event observed to be produced."""
    while True:
      if not self.interaction_queue.empty():
        print(len(self.interaction_queue.queue))
        event = self.interaction_queue.get()
        self.simulate_interaction_event(event)
        self.interaction_queue.task_done()


class WindowNotVisibleError(Exception):
  """Class providing a custom error for window not visible."""

  def __init__(self, message="Window is not visible"):
    self.message = message
    super().__init__(self.message)


if __name__ == "__main__":
  WindowSelector = WindowSelection()
  window_title, window_hwnd = WindowSelector.select()
  print(f"Selected window: {window_title[0]} ")

  interaction_simulator = InteractionSimulator()
  interaction_simulator.bring_to_foreground(window_hwnd)
  interaction_simulator.mouse_click(window_hwnd, 200, 200, "left")
  interaction_simulator.key_press(ord("a"))
