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
"""Module providing a window capturing for windows."""

import ctypes
import threading
import cv2
import numpy as np
import PIL.Image
import pygetwindow as gw
import win32con
import win32gui
import win32ui
from window_contour_drawer import WindowContourDrawer
from window_selection import WindowSelection


class WindowCapture:
  """Attributes: title: title of the window you need to capture.

  hwnd: window handle.
  """

  def __init__(self, title=None):
    self.window_title = title

    if not title:
      raise ValueError('No window title provided!')

    self.hwnd = self.get_window_handle(self.window_title)

    if not self.hwnd:
      raise ValueError('Window not found!')

    self.drawer = WindowContourDrawer(self.hwnd)
    self.drawer_thread = threading.Thread(
        target=self.drawer.start_drawing, args=()
    )
    self.drawer_thread.start()

  def get_window_handle(self, name):
    """get window handle.

    Args:
      name: window name.

    Returns:
      window handle.
    """
    window_list = []
    win32gui.EnumWindows(
        lambda hwnd, wndList: wndList.append(
            (win32gui.GetWindowText(hwnd), hwnd)
        ),
        window_list,
    )

    for pair in window_list:
      if name in pair[0]:
        return pair[1]
    return None

  def screenshot(self):
    """function to generate screeshoots.

    Returns:
      the screenshot of the window.
    """

    try:
      dpi = ctypes.windll.user32.GetDpiForWindow(self.hwnd)
      ratio = dpi / 96
      l, t, r, b = win32gui.GetClientRect(self.hwnd)
      sl, st, _, _ = win32gui.GetWindowRect(self.hwnd)
      cl, ct = win32gui.ClientToScreen(self.hwnd, (l, t))
      border = 8

      self.size = (int((r - l) * ratio), int((b - t) * ratio))
      position = (int((cl - sl) * ratio) - border, int((ct - st) * ratio))

      hdc = win32gui.GetDC(self.hwnd)
      windowdc = win32ui.CreateDCFromHandle(hdc)
      newdc = windowdc.CreateCompatibleDC()
      bitmap = win32ui.CreateBitmap()

      bitmap.CreateCompatibleBitmap(windowdc, self.size[0], self.size[1])
      newdc.SelectObject(bitmap)

      newdc.BitBlt((0, 0), self.size, windowdc, position, win32con.SRCCOPY)
      bitmap.Paint(newdc)

      self.bmpstr = bitmap.GetBitmapBits(True)

      screenshot = np.frombuffer(self.bmpstr, dtype=np.uint8).reshape(
          self.size[1], self.size[0], 4
      )  # 4 channels because of BGRX

      if ratio != 1:
        screenshot = cv2.resize(screenshot, None, fx=1 / ratio, fy=1 / ratio)

      newdc.DeleteDC()
      windowdc.DeleteDC()
      win32gui.ReleaseDC(self.hwnd, hdc)
      win32gui.DeleteObject(bitmap.GetHandle())
      return screenshot

    except win32ui.error as e:
      raise ScreenCaptureError(f'CreateCompatibleDC failed: {e}') from e

  def save(self, path='test.png'):
    """function to save screenshoot to png.

    Args:
      path: path and filename this represent the location and filename that
        needs to be saved

    Returns:
      window dimensions as (with,height) or (None,None) if the window
    does not exist
    """
    try:
      im = PIL.Image.frombuffer(
          'RGB', self.size, self.bmpstr, 'raw', 'BGRX', 0, 1
      )
      im.save(path, 'PNG')
    except IOError as exc:
      raise IOError('Screenshoot not found!') from exc

  def get_window_dimensions(self):
    """Retrieves the dimensions of the specified window.

    Returns:
      tuple: The dimensions of the window (width, height), or (None, None) if
      the window does not exist.
    """
    windows = gw.getWindowsWithTitle(self.window_title)
    if windows:
      window = windows[0]  # Take the first window that matches the title
      return window.width, window.height
    return None, None

  def __del__(self):
    """Destructor to ensure the contour drawer thread is stopped when the object is destroyed."""
    if self.drawer_thread and self.drawer_thread.is_alive():
      self.drawer.stop_drawing()
      self.drawer_thread.join()


class ScreenCaptureError(Exception):
  """Custom exception for screencapture."""

  def __init__(
      self, message='the incoming streaming of serialized images had a problem'
  ):
    self.message = message
    super().__init__(message)


if __name__ == '__main__':
  WindowSelector = WindowSelection()
  window_title, window_handle = WindowSelector.select()
  print(f'Selected window: {window_title}')

  captured_window = WindowCapture(window_title)
  captured_window.screenshot()
  captured_window.save()

  input('click anything to destroy capture object and stop seeing red contour ')
