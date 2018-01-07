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

# Building and Running with Docker

* Open main folder in Unity, open `File > Build Settings`.
* Build `Scenes/desert` into `./Build/Desert/start.exe`.
* Run `Desert/start.exe` multiple times

To build the server

* Open main folder in Unity, open `File > Build Settings`.
* Change build settings to `Linux` + `x86 + x86_x64 (Universal)` + `Headless Mode: true`.
* Open `Player Settings` and set `Scripting Define Symbols` to `CROSS_PLATFORM_INPUT;DEDICATED_SERVER_MODE`.
* Build `Scenes/server` into `./Build/Server/Linux/linuxserver.x86`.
* Build and Run with docker (from root folder): 
```sh
docker build -t unitydronesserver:latest .
docker run -p 5701:5701/udp unitydronesserver:latest
```
* Check that the docker container is running:
```sh
docker ps -a
```
* Start multiple clients and connect

# Run on Kubernetes on Azure

You can run **Building and Running with Docker** to create a new image, then run the following commands:
(Replace *<USER_NAME>* with your docker hub user name)
```sh
docker tag unitydronesserver:latest <USER_NAME>/unitydronesserver:latest
docker push <USER_NAME>/unitydronesserver:latest
```

Follow instructions to setup a new kubernetes cluster on Azure under: [k8s/setup.sh](k8s/setup.sh).

Deploy a new application using:
```sh
kubectl create -f k8s/app.yaml
```

If you want to use your own image, change [k8s/app.yaml](k8s/app.yaml) and replace *morshemesh* with your own user name.

* Wait until the Service is done deploying on kubernetes and has a Public IP
* Use that Public IP and set `server_ip` under `Assets/Scripts/Client.cs`.
* Build/Run the client for Windows.

# Resources

* [Unity flying drone tutorial from scratch (10 videos)](https://www.youtube.com/watch?v=3R_V4gqTs_I)
* [Adding SteamVR Teleporting abilities to scene](https://www.youtube.com/watch?v=Zd0OXk_7sx8)

# License
MIT License