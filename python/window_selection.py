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
"""Module providing a window selection."""

import os
import pygetwindow as gw
import win32gui


class WindowSelection:
  """Attributes: title: title of the window you need to capture.

  hwnd: window handle. size: window size. position: window position.
  """

  def __init__(self):
    pass

  def __list_open_windows(self):
    windows = gw.getAllWindows()
    window_info = [win.title for win in windows if win.visible and win.title]
    return window_info

  def __get_window_handle(self, name):
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

  def select(self):
    """Method that generates a list of windows and allow the user to select one.

    Returns:
      the title of the selected window.
    """
    windows_info = self.__list_open_windows()

    # If no windows found, exit
    if not windows_info:
      print('No visible windows found.')
      return None, None
    # Display available windows for selection
    for i, (title) in enumerate(windows_info, 1):
      print(f'{i}. {title}')
    # Allow user to select a window
    while True:
      try:
        idx = int(
            input('\nEnter the number of the window you want to capture: ')
        )
        if 1 <= idx <= len(windows_info):
          break
        print('Invalid number. Please try again.')
      except ValueError:
        print('Please enter a valid number.')
    selected_title = windows_info[idx - 1]
    handle = self.__get_window_handle(selected_title)

    if not self.ask_user_permission():
      selected_title = None
      handle = None

    return selected_title, handle

  def ask_user_permission(self):
    """Display a notice to the user.

    This function presents a warning to the user about the potential risks.

    Returns:
        bool: True if the user gives consent to capture the window, False
        otherwise.
    """

    os.system('cls')
    print('###########################################################')
    print(' WARNING: EXPOSING SENSITIVE INFO OVER INSECURE CONNECTION ')
    print('###########################################################')
    print('This will capture and insecurely transmit any')
    print('sensitive information displayed on your windows,')
    print('including sensitive information such as passwords,')
    print('payment info, photos, and messages. This will be')
    print('insecurely transmitted as jpgs that can be intercepted.')
    print('##########################')
    user_response = input(
        'Type [y] and press Enter to give consent, or any other key to'
        ' decline: '
    )
    os.system('cls')
    return user_response.lower() == 'y'


if __name__ == '__main__':
  WindowSelector = WindowSelection()
  window_title = WindowSelector.select()
  print(f'Selected window: {window_title[0]} ')
