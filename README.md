# Unity Drones Multiplayer

A sample project to use UNET protocol between client and server to allow multiplayer support for Unity by enabling multiple users to connect and control a drone while watching other players interact with their own drones.

![Preview](/docs/preview.gif)

# Execution on Windows

* Open main folder in Unity, open `File > Build Settings`.
* Build `Scenes/desert` into `./Build/Desert/start.exe`.
* Build `Scenes/server` into `./Build/Server/start.exe`.
* Run `Server/start.exe`
* Run `Desert/start.exe` multiple times
* Connect with a different username on each desert window
* Interact in one window to watch the drone fly in the others as well

You can also execute the server by opening `Scenes/server` in unity and pressing `Play`.

# License
MIT License