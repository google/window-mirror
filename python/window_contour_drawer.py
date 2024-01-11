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
"""This module provides functionality to draw a persistent red contour around a specified window."""

import time
import win32api
import win32gui
import win32ui
from window_selection import WindowSelection


class WindowContourDrawer:
  """This class is responsible for drawing a persistent red contour around a window specified by its handle."""

  def __init__(self, hwnd):
    """Initializes the object with the specified window handle.

    Args:
        hwnd (str): window handle.

    Raises:
        Exception: If the specified window is not found.
    """
    self.hwnd = hwnd
    self._is_drawing = False

  def red_draw_contour_window(self, hdc, rect):
    """Draws a red contour around the specified window.

    Args:
        hdc (handle): A handle to the window.
        rect (tuple): A tuple defining the conour rectangle (left, top, right,
          bottom).
    """

    left, top, right, bottom = rect
    dc_obj = win32ui.CreateDCFromHandle(hdc)
    try:
      red_pen = win32ui.CreatePen(1, 2, win32api.RGB(255, 0, 0))
      dc_obj.SelectObject(red_pen)

      # Drawing the contour
      dc_obj.MoveTo((left, top))
      dc_obj.LineTo((right, top))
      dc_obj.LineTo((right, bottom))
      dc_obj.LineTo((left, bottom))
      dc_obj.LineTo((left, top))

    except win32ui.error as e:
      print(f"Error: {e}")

    finally:
      try:
        dc_obj.DeleteDC()
      except win32ui.error as e:
        print(f"Error deleting device context: {e}")

  def persistent_draw(self):
    """Continuously draws and updates the red contour.

    This method runs a loop that constantly checks the window's state and
    redraws the contour.
    """
    while self._is_drawing and win32gui.IsWindow(self.hwnd):
      rect = win32gui.GetWindowRect(self.hwnd)
      width, height = rect[2] - rect[0], rect[3] - rect[1]

      # Draw on the window's device context
      hdc = win32gui.GetWindowDC(self.hwnd)
      self.red_draw_contour_window(hdc, (10, 1, width - 10, height - 10))
      win32gui.ReleaseDC(self.hwnd, hdc)

      time.sleep(0.5)  # Redraw every 500ms

  def start_drawing(self):
    """Starts the contour drawing process."""
    self._is_drawing = True
    self.persistent_draw()

  def stop_drawing(self):
    """Stops the contour drawing process."""
    self._is_drawing = False


if __name__ == "__main__":
  WindowSelector = WindowSelection()
  window_title, window_handle = WindowSelector.select()
  print(f"Selected window: {window_title}")

  drawer = WindowContourDrawer(window_handle)
  drawer.start_drawing()
