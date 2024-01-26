# WindowMirror

Streaming windows via WebSockets peer-to-peer

this only works on Windows devices and uses the win32 API

Install the requirements.txt :

```
$ pip install -r requirements.txt
```

1) Run ip config and get your ip address. Update the ip address in
stream_receiver.py. Run stream_receiver.py to start the receiving client.

2) Use the annotated ip and edit it in the streaming_client.py .

3) While stream_receiver.py is still running open a new terminal and run
streaming_client.py.

4) Select a window you would like to share by inserting the corresponding number
and press enter. The window texture will open on an opencv window. Note that
Only non-hardware-accelerated windows can be shared !!! \
Some programs like Visual Studio Code or chrome can be run without hardware
acceleration, google it for more info

TEST ACROSS TWO WINDOWS MACHINES ON THE SAME NETWORK

6) Now that you tested it locally you can test it across two laptops on the same
network Make sure both windows machine and the machine in which you run unity
are in the same network and that the networks allows peer to peer communication.


## Contributors

 - **Riccardo Bovo (Imperial College London)** - Student Researcher at Google AR & VR 
 - **Li-Te Cheng (Google AR & VR)** 
 - **Mar Gonzalez-Franco (Google AR & VR)** 
 - **Eric J Gonzalez (Google AR & VR)** 
