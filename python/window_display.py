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
"""Module providing the display capturing for windows for input simulation testing purposes."""

import queue
import threading
import cv2
from events import UIevent
from events import UIEventsTypes


class WindowDisplay:
  """A simple class to display the images sent during peer-to-peer.

  Attributes:
    img: title of the window you need to capture.
    window_id: the window_id.
    interaction_queue: ui events queue.
    work_queue: events sent by an external object (image updates).
    close: boolean that is check at every updated to close the displayer.
    ocr: alto xml file outputted from the optical recognition character OCR.
    img_updated: updated image to be displayed.

  hwnd: window handle. size: window size. position: window position.
  """

  def __init__(self, img, window_id, work_queue, interaction_queue):
    self.img = img
    self.window_id = window_id
    self.interaction_queue = interaction_queue
    self.work_queue = work_queue
    self._running = True
    self.img_updated = True  # Flag to track image updates

  def updateimg(self, img):
    """update the img to be displayed."""
    self.img = img
    self.img_updated = True  # Set flag when image is updated

  def quit(self, value):
    """quit the display."""
    self._running = not value

  def display(self):
    """open a cv2 image show and bind the mouseclick event."""
    cv2.namedWindow(self.window_id)
    cv2.setMouseCallback(self.window_id, self.on_mouse)

    while self._running:
      self._process_frame()

      cv2.imshow(self.window_id, self.img)

      k = cv2.waitKey(1)
      if k != -1:
        event_to_send = UIevent(
            UIEventsTypes(UIEventsTypes.KEYSTROKE), k, 0, 0, self.window_id
        )
        self.interaction_queue.put(event_to_send)

    cv2.destroyWindow(self.window_id)

  def _process_frame(self):
    """process queue if needed."""
    if not self.work_queue.empty():
      task, param = self.work_queue.get()
      task(param)
      self.work_queue.task_done()

  def on_mouse(self, event, x, y, p1, _):
    """handles the envent triggered by the mouseclick and triggers the queue event.

    Args:
       event: event name
       x: x texture coordinates
       y: y texture coordinates
       p1: ? cv2 parameters
       _: ? cv2 paramenters
    """

    if (
        event == cv2.EVENT_LBUTTONDBLCLK
        or event == cv2.EVENT_RBUTTONDBLCLK
        or event == cv2.EVENT_MBUTTONDBLCLK
    ):
      event_to_send = UIevent(UIEventsTypes(event), 0, x, y, self.window_id)
      self.interaction_queue.put(event_to_send)

    elif event == cv2.EVENT_MOUSEWHEEL:
      event_to_send = UIevent(UIEventsTypes(event), p1, x, y, self.window_id)
      self.interaction_queue.put(event_to_send)

    elif (
        event == cv2.EVENT_LBUTTONDOWN
        or event == cv2.EVENT_RBUTTONDOWN
        or event == cv2.EVENT_MBUTTONDOWN
    ):
      event_to_send = UIevent(UIEventsTypes(event), 0, x, y, self.window_id)
      self.interaction_queue.put(event_to_send)


class UIEventObserver:
  """A simple class to check that async event receiveing from mouse.

  Attributes:
    interaction_queue: ui events queue.
  """

  def __init__(self, interaction_queue):
    self.interaction_queue = interaction_queue

  def wait_for_event(self):
    """wait for the event observed to be produced."""
    while True:
      if not self.interaction_queue.empty():
        event = self.interaction_queue.get()
        print(event)
        self.interaction_queue.task_done()


# Start of the main program here
if __name__ == '__main__':
  update_queue = queue.Queue()
  interact_queue = queue.Queue()

  # for testing purposes you can generate test.png launching window_capture
  # alternatively you can insert the name/path to any png file for image_name
  # and for other_image_name, w_id does not require to be changed

  image_name = 'test.png'
  w_id = '1920393'
  loaded_image = cv2.imread(image_name)
  LI = WindowDisplay(loaded_image, w_id, update_queue, interact_queue)

  # Create an instance of EventObserver and pass the producer as a parameter
  observer = UIEventObserver(interact_queue)

  # Create separate threads for the producer and observer
  image_thread = threading.Thread(target=LI.display)
  observer_thread = threading.Thread(target=observer.wait_for_event)

  # Start both threads
  image_thread.start()
  observer_thread.start()

  # replace image
  input('now?')
  other_image_name = 'test1.png'
  other_loaded_image = cv2.imread(other_image_name)
  update_queue.put((LI.updateimg, other_loaded_image))

  # replace image
  input('hit a button to close')
  update_queue.put((LI.quit, True))

  # Wait for both threads to finish
  image_thread.join()
  observer_thread.join()
