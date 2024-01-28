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
"""Module which abstraction for the events."""

import enum
import cv2
import numpy as np


class UIEventsTypes(enum.Enum):
  LEFT_BUTTON_DOWN = cv2.EVENT_LBUTTONDOWN
  RIGHT_BUTTON_DOWN = cv2.EVENT_RBUTTONDOWN
  MIDDLE_BUTTON_DOWN = cv2.EVENT_MBUTTONDOWN
  LEFT_DOUBLE_CLICK = cv2.EVENT_LBUTTONDBLCLK
  RIGHT_DOUBLE_CLICK = cv2.EVENT_RBUTTONDBLCLK
  MIDDLE_DOUBLE_CLICK = cv2.EVENT_MBUTTONDBLCLK
  SCROLL = cv2.EVENT_MOUSEWHEEL
  KEYSTROKE = 11
  UNINITIALIZED = 0


class MouseButton(enum.Enum):
  LEFT = 0
  RIGHT = 1
  MIDDLE = 2


class ClickType(enum.Enum):
  MOUSE_CLICK = 0
  MOUSE_DOUBLE_CLICK = 1
  MOUSE_SCROLL = 2
  KEY_PRESS = 3


class UIevent:
  """this is an abstraction class for the events.

  Attributes:
    event_type: the type of the event UIEventsTypes,
    value: the value of the keystroke or the scroll value,
    x: px value of the mouse if this is a mouse event,
    y: px value of the mouse if this is a mouse event,
    window_id: window id hwnd win 32 api,
    data_array: an array containing the above,
  """

  def __init__(self, event_type=0, value=0, x=0, y=0, window_id=0):
    self.event_type = UIEventsTypes(event_type).value
    self.value = value
    self.x = x
    self.y = y
    self.window_id = int(window_id)
    self.data_array = np.array(
        [self.event_type, self.value, self.x, self.y, self.window_id],
        dtype=np.int32,
    )

  def to_bytes(self):
    """transform event in bytes."""
    return self.data_array.tobytes()

  def from_bytes(self, inbytes):
    """handles byte to event."""
    self.data_array = np.frombuffer(inbytes, dtype=np.int32)
    self.event_type = self.data_array[0]
    self.value = self.data_array[1]
    self.x = self.data_array[2]
    self.y = self.data_array[3]
    self.window_id = self.data_array[4]

  def is_valid(self):
    """returns true is event is valid."""
    if self.event_type == UIEventsTypes.KEYSTROKE.value:
      return True if self.value != 0 else False
    elif self.event_type == UIEventsTypes.SCROLL.value:
      if (
          self.value != 0
          and self.x != 0
          and self.y != 0
          and self.window_id != 0
      ):
        return True
      else:
        return False
    else:
      return (
          True if self.x != 0 and self.y != 0 and self.window_id != 0 else False
      )

  def __str__(self):
    """format event printing."""
    to_print = f"{UIEventsTypes(self.event_type).name}"
    to_print += f"  window_id = {self.window_id} "
    if self.event_type == UIEventsTypes.KEYSTROKE.value:
      to_print += f" | value = {chr(self.value)},"
    elif self.event_type == UIEventsTypes.SCROLL.value:
      to_print += f"| value = {self.value} ,"
      to_print += f"x = {self.x},  y = {self.y},  "
    else:
      to_print += f"x = {self.x},  y = {self.y},  "
    return to_print

  def button(self) -> MouseButton:
    """return mouse button."""
    if UIEventsTypes(self.event_type) == UIEventsTypes.LEFT_BUTTON_DOWN:
      return MouseButton.LEFT
    elif UIEventsTypes(self.event_type) == UIEventsTypes.LEFT_DOUBLE_CLICK:
      return MouseButton.LEFT
    elif UIEventsTypes(self.event_type) == UIEventsTypes.RIGHT_BUTTON_DOWN:
      return MouseButton.RIGHT
    elif UIEventsTypes(self.event_type) == UIEventsTypes.RIGHT_DOUBLE_CLICK:
      return MouseButton.RIGHT
    elif UIEventsTypes(self.event_type) == UIEventsTypes.MIDDLE_BUTTON_DOWN:
      return MouseButton.MIDDLE
    elif UIEventsTypes(self.event_type) == UIEventsTypes.MIDDLE_DOUBLE_CLICK:
      return MouseButton.MIDDLE
    else:
      return None

  def event_task(self) -> ClickType:
    """return click/scroll/press type."""
    if UIEventsTypes(self.event_type) == UIEventsTypes.LEFT_BUTTON_DOWN:
      return ClickType.MOUSE_CLICK
    elif UIEventsTypes(self.event_type) == UIEventsTypes.RIGHT_BUTTON_DOWN:
      return ClickType.MOUSE_CLICK
    elif UIEventsTypes(self.event_type) == UIEventsTypes.MIDDLE_BUTTON_DOWN:
      return ClickType.MOUSE_CLICK
    elif UIEventsTypes(self.event_type) == UIEventsTypes.LEFT_DOUBLE_CLICK:
      return ClickType.MOUSE_DOUBLE_CLICK
    elif UIEventsTypes(self.event_type) == UIEventsTypes.RIGHT_DOUBLE_CLICK:
      return ClickType.MOUSE_DOUBLE_CLICK
    elif UIEventsTypes(self.event_type) == UIEventsTypes.MIDDLE_DOUBLE_CLICK:
      return ClickType.MOUSE_DOUBLE_CLICK
    elif UIEventsTypes(self.event_type) == UIEventsTypes.SCROLL:
      return ClickType.MOUSE_SCROLL
    elif UIEventsTypes(self.event_type) == UIEventsTypes.KEYSTROKE:
      return ClickType.KEY_PRESS
    else:
      return None


# Start of the main program here
if __name__ == "__main__":
  event_to_send = UIevent(
      UIEventsTypes.KEYSTROKE.value, ord("a"), 0, 0, "102939483"
  )
  print(event_to_send)
  print(event_to_send.data_array)

  received_data = event_to_send.to_bytes()
  print(received_data)

  received_event = UIevent()
  received_event.from_bytes(received_data)
  print(received_event)
  print(received_event.is_valid())
