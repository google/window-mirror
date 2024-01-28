## Test Unity-packge Locally

This package can be tested in Unity editor in conjunction with python
windowmirror module running on the same windows machine in a terminal window
Please complete the instructions for the setup of the python windowmirror module
before starting.

In Unity:

* Create a new unity project in Unity and import the com.google.xr.windowmirror
"Window" > "Package Manager" > "Add package from disk"

* Once the package is loaded import the package samples and open the scene
"Editor Screen testing"

* Make sure that the component "Receiver" attached to the gameobject
"Screens" as the "Ip address" value set to 127.0.0.1

* Then press Play

In a terminal window:

* From the python windowmirror module run streamingClient.py on the same
machine (needs to be a windows machine)

* Set the target ip of streamingClient.py to "127.0.0.1" and run 

* Follow the instruction on the terminal and choose what window to share with
the unity application


