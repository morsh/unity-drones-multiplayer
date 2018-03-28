using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player
{
    public int connectionId;
    public string playerName;
    public GameObject avatar;
    public Vector3 position;
    public Vector3 velocity;
    public Quaternion rotation;
}

public class Client : MonoBehaviour {

    #region Constants
    const string DISPLAY_OPT_3D = "3D Display";
    const string DISPLAY_OPT_WMR = "Windows Mixed Reality";
    #endregion

    #region Private Properties
    private const int MAX_CONNECTIONS = 100;

    private int hostId;
    
    private int reliableChannel;
    private int unreliableChannel;

    private int selfClientId = -1;
    private int connectionId;

    private float connectionTime;
    private bool isConnected = false;
    private bool isStarted = false;
    private byte error;

    private string playerName;
    private string status = string.Empty;
    private string renderingMethod = DISPLAY_OPT_3D;

    private Dictionary<int, Player> players = new Dictionary<int, Player>();
    #endregion

    #region Public Members
    public string server_ip = "127.0.0.1";
    public int port = 5701;

    public GameObject playerPrefab;
    public bool isMultiView = false;
    public GameObject view3D;
    public GameObject viewWMR;
    public GameObject connectionCanvas;
    public Text statusText;
    public Text playersCount;
    #endregion

    public void Start()
    {
        Application.runInBackground = true;
        SetStatus(string.Empty);

        if (isMultiView) {
            view3D.SetActive(true);
            viewWMR.SetActive(true);
        } else if (connectionCanvas == null) {
            Connect();
        }
    }       

    public void Connect()
    {
        // Does the player has a name
        string playerNameInput = string.Empty;

        if(connectionCanvas != null) {
            playerNameInput = GameObject.Find("NameInput").GetComponent<InputField>().text;
            if(playerNameInput == "") {
                Debug.Log("You must enter a name");
                return;
            }

            if(isMultiView) {
                var dropdown = GameObject.Find("RenderingMethodDropDown").GetComponent<Dropdown>();
                renderingMethod = dropdown.options[dropdown.value].text;
            }
        }

        playerName = playerNameInput;

        SetStatus("Connecting...");
        NetworkTransport.Init();
        ConnectionConfig connectionConfig = new ConnectionConfig();

        // Client and server need to have the same connection types
        reliableChannel = connectionConfig.AddChannel(QosType.Reliable);
        unreliableChannel = connectionConfig.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(connectionConfig, MAX_CONNECTIONS);

        hostId = NetworkTransport.AddHost(topo, 0);
        connectionId = NetworkTransport.Connect(hostId, server_ip, port, 0, out error);

        connectionTime = Time.time;
        isConnected = true;
        SetStatus("Connected");
    }

    private void Update()
    {
        if (!isConnected) { return; }

        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        switch (recData)
        {
            case NetworkEventType.DataEvent:       //3
                string message = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log("Receiving : " + message);

                var parts = message.Split('|');

                switch (parts.Length > 0 ? parts[0] : "")
                {
                    case "ASKNAME":
                        OnAskName(parts);
                        break;

                    case "UPD":
                        SpawnPlayer(int.Parse(parts[1]), parts[2]);
                        break;

                    case "DC":
                        PlayerDisconnected(int.Parse(parts[1]));
                        break;

                    case "ASKPOSITION":
                        OnAskPosition(parts);
                        break;

                    default:
                        Debug.Log("Invalid message : " + message);
                        break;
                }
                break;
        }

        // Update players positions
        foreach (var player in players.Values)
        {
            if (player.connectionId != selfClientId)
            {
                player.avatar.transform.position = Vector3.Lerp(player.avatar.transform.position, player.position, 0.1f);
                player.avatar.transform.rotation = Quaternion.Lerp(player.avatar.transform.rotation, player.rotation, 0.1f);
            }
        }

        if (statusUpdateTime != -1 && (Time.time - statusUpdateTime > statusResetRate))
        {
            SetStatus(string.Empty);
        }
    }

    private void Send(string message, int channelId)
    {
        Debug.Log("Sending : " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        NetworkTransport.Send(hostId, connectionId, channelId, msg, message.Length * sizeof(char), out error);

    }

    private void OnAskName(string[] data)
    {
        // Set self client's id
        selfClientId = int.Parse(data[1]);

        if (playerName == string.Empty) {
            playerName = "Player " + selfClientId.ToString();
        }

        // Send self name to server
        Send("NAMEIS|" + playerName, reliableChannel);

        // Create all the other players
        for (int i = 2; i < data.Length - 1; i++)
        {
            var otherClientId = int.Parse(data[i].Substring(0, data[i].IndexOf('%')));
            var otherClientName = data[i].Substring(data[i].IndexOf('%') + 1);
            SpawnPlayer(otherClientId, otherClientName);
        }
    }

    private void SpawnPlayer(int id, string name)
    {
        GameObject go = Instantiate(playerPrefab) as GameObject;
        
        var player = new Player();
        player.connectionId = id;
        player.playerName = name;
        player.avatar = go;
        player.avatar.GetComponentInChildren<TextMesh>().text = name;

        // Is this ours
        if (id == selfClientId)
        {
            // Hide connection canvas
            if(connectionCanvas) {
                connectionCanvas.SetActive(false);
            }

            // Add mobility
            if (isMultiView && renderingMethod == DISPLAY_OPT_WMR || (viewWMR != null && view3D == null))
            {
                AddWMRMobility(player);
            } 
            else
            {
                Add3DMobility(player);
            }

            isStarted = true;
            player.avatar.tag = "Player";
        }
        else
        {
            // Remove gravity and mess, since they are controlled by other player/environment
            var playerRigidbody = player.avatar.GetComponent<Rigidbody>();
            playerRigidbody.mass = 1;
            playerRigidbody.useGravity = false;

            SetStatus("New player logged in: " + player.playerName);
        }

        players.Add(id, player);
        SetPlayersCount();
    }

    private void AddWMRMobility(Player player)
    {
        if(view3D != null) { view3D.SetActive(false); }
        if(viewWMR != null) { viewWMR.SetActive(true); }

        var movementScript = player.avatar.AddComponent<DroneMovementScript>();

        var wmrView = GameObject.Find("WMRView");
        //wmrView.transform.SetParent(player.avatar.transform);
        //wmrView.transform.position = new Vector3(0, 0, 0);
        //wmrView.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));

        //var camera = GameObject.Find("MixedRealityCameraParent");
        //camera.transform.position = new Vector3(0, 0, -1);
        //camera.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));

        var cursorVisual = GameObject.Find("CursorVisual");
        cursorVisual.SetActive(false);

        var script = wmrView.AddComponent<WMRCameraFollowScript>();
        script.drone = player.avatar;
        script.angle = 4;
        script.behindPosition = new Vector3(0, 0, -2);
    }

    private void Add3DMobility(Player player)
    {
        if(view3D != null) { view3D.SetActive(true); }
        if(viewWMR != null) { viewWMR.SetActive(false); }

        player.avatar.AddComponent<DroneMovementScript>();
        var camera = GameObject.Find("3D Camera");
        var script = camera.AddComponent<CameraFollowScript>();
        script.drone = player.avatar;
        script.angle = 18;
    }

    private void PlayerDisconnected(int connectionId)
    {
        if (!players.ContainsKey(connectionId)) { return; }

        var playerName = players[connectionId].playerName;
        Destroy(players[connectionId].avatar);
        players.Remove(connectionId);
        SetPlayersCount();
        SetStatus("Player " + playerName + " has disconnected");
    }

    private void OnAskPosition(string[] data)
    {
        // Update everyone else
        for (int i = 1; i < data.Length; i++)
        {
            ServerClient state = ServerClient.LoadPosition(data[i]);

            if (state.connectionId != selfClientId && players.ContainsKey(state.connectionId))
            {
                players[state.connectionId].position = state.position;
                players[state.connectionId].velocity = state.velocity;
                players[state.connectionId].rotation = state.rotation;
            }
        }

        // Send self position
        if (players.ContainsKey(selfClientId))
        {
            var selfState = new ServerClient
            {
                connectionId = selfClientId,
                playerName = playerName,
                position = players[selfClientId].avatar.transform.position,
                velocity = players[selfClientId].avatar.GetComponent<Rigidbody>().velocity,
                rotation = players[selfClientId].avatar.transform.rotation,
            };
            Send("UPDPOSITION|" + selfState.ToStateString(), unreliableChannel);
        }
    }

    private float statusUpdateTime = -1;
    private float statusResetRate = 5.0f; // Reset status every 5 seconds
    private void SetStatus(string message)
    {
        if (statusText == null) { return; }

        if (message == string.Empty)
        {
            statusText.text = message;
            statusUpdateTime = -1;
        }
        else
        {
            statusText.text = "[" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "] " + message;
            statusUpdateTime = Time.time;
        }
    }

    private void SetPlayersCount() {
        if(playersCount != null) {
            playersCount.text = players.Count.ToString();
        }
    }
}
