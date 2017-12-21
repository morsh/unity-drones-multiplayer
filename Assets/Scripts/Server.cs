using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour
{
    private const int MAX_CONNECTIONS = 100;

    private int port = 5701;

    private int hostId;
    private int webHostId;

    private int reliableChannel;
    private int unreliableChannel;

    private bool isStarted = false;
    private byte error;

    private List<ServerClient> clients = new List<ServerClient>();

    private float lastMovementUpdate;
    private float movementUpdateRate = 0.05f;

    private void Start()
    {
        Application.runInBackground = true;

        NetworkTransport.Init();
        ConnectionConfig connectionConfig = new ConnectionConfig();

        // Client and server need to have the same connection types
        reliableChannel = connectionConfig.AddChannel(QosType.Reliable);
        unreliableChannel = connectionConfig.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(connectionConfig, MAX_CONNECTIONS);

        hostId = NetworkTransport.AddHost(topo, port, null);

        // Enable browsers to connect to the game
        webHostId = NetworkTransport.AddWebsocketHost(topo, port, null);

        isStarted = true;
    }

    private void Update()
    {
        if (!isStarted) { return; }

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
            case NetworkEventType.ConnectEvent:    //2
                OnConnection(connectionId);
                Debug.Log("Player " + connectionId + " has connected");
                break;
            case NetworkEventType.DataEvent:       //3
                string message = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log("Receving from " + connectionId + " has sent : " + message);

                var parts = message.Split('|');

                switch (parts.Length > 0 ? parts[0] : "")
                {
                    case "NAMEIS":
                        OnNameIs(connectionId, parts);
                        break;

                    case "UPDPOSITION":
                        OnUpdatePosition(connectionId, parts[1]);
                        break;

                    default:
                        Debug.Log("Invalid message : " + message);
                        break;
                }
                break;
            case NetworkEventType.DisconnectEvent: //4
                OnPlayerDisconnected(connectionId);
                Debug.Log("Player " + connectionId + " has disconnected");
                break;
        }

        // Ask player for their position
        if (Time.time - lastMovementUpdate > movementUpdateRate)
        {
            lastMovementUpdate = Time.time;

            var message = "ASKPOSITION|";
            clients.ForEach(c => message += c.ToStateString() + "|");
            Send(message.Trim('|'), unreliableChannel, clients);
        }
    }

    private void OnPlayerDisconnected(int connectionId)
    {
        // Remove player from client list
        clients.Remove(clients.Find(c => c.connectionId == connectionId));

        // Tell everyone a player has disconnected
        Send("DC|" + connectionId, reliableChannel, clients);
    }

    private void OnConnection(int connectionId)
    {
        // Add user to a list
        ServerClient client = new ServerClient();
        client.connectionId = connectionId;
        client.playerName = "TEMP";
        clients.Add(client);

        // When the platey joins the server, tell him his id
        // Reequest his name and send the name of all the other players
        string message = "ASKNAME|" + connectionId + "|";
        foreach (var serverClient in clients)
        {
            message += serverClient.connectionId + "%" + serverClient.playerName + "|";
        }

        message = message.Trim('|');

        // Expected message:
        // ASKNAME|3|DAVE%1|LINA%2|TEMP%3
        Send(message, reliableChannel, connectionId);
    }

    private void Send(string message, int channelId, int connectionId)
    {
        List<ServerClient> clientsArray = new List<ServerClient>();
        clientsArray.Add(clients.Find(c => c.connectionId == connectionId));
        Send(message, channelId, clientsArray);
    }

    private void Send(string message, int channelId, List<ServerClient> clients)
    {
        Debug.Log("Sending : " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);

        foreach (var client in clients)
        {
            NetworkTransport.Send(hostId, client.connectionId, channelId, msg, message.Length * sizeof(char), out error);
        }

    }

    private void OnNameIs(int connectionId, string[] data)
    {
        var playerName = data[1];

        // Link the name to the connection Id
        var clientToUpdate = clients.Find(c => c.connectionId == connectionId);
        clientToUpdate.playerName = playerName;

        // Tell everyone that a new player has connected
        Send("UPD|" + connectionId + '|' + playerName, reliableChannel, clients);
    }

    private void OnUpdatePosition(int connectionId, string state)
    {
        var newState = ServerClient.LoadPosition(state);
        var client = clients.Find(c => c.connectionId == connectionId);
        client.position = newState.position;
        client.velocity = newState.velocity;
        client.rotation = newState.rotation;
    }
}
