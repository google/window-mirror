# windowmirror

This application works in the Unity editor in conjunction with python
windowmirror application running on the same machine in a terminal window.
Please complete the instructions for the setup of the python windowmirror module
before starting.

In Unity:

* Create a new unity project in Unity and import the com.google.xr.windowmirror
"Window" > "Package Manager" > "Add package"

* Once the package is loaded import the package samples and open the scene
"Editor Screen testing"

* Make sure that the component "Receiver" attached to the gameobject
"Screens" as the "Ip address" value set to 127.0.0.1

* Then press Play

In a terminal window:

* From the python windowmirror module run streamingClient.py on the same
machine (needs to be a windows machine)

* Set the target ip of streamingClient.py to "127.0.0.1"

* Follow the instruction on the terminal and choose what window to share with
the unity application
