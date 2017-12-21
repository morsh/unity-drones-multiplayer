using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ServerClient
{
    public int connectionId;
    public string playerName;
    public Vector3 position;
    public Vector3 velocity;
    public Quaternion rotation;

    public string ToStateString()
    {
        return connectionId.ToString() + 
            "%" + position.x.ToString() + "%" + position.y.ToString() + "%" + position.z.ToString() +
            "%" + velocity.x.ToString() + "%" + velocity.y.ToString() + "%" + velocity.z.ToString() +
            "%" + rotation.x.ToString() + "%" + rotation.y.ToString() + "%" + rotation.z.ToString() + "%" + rotation.w.ToString();
    }

    static public ServerClient LoadPosition(string state)
    {
        var data = state.Split('%');
        return new ServerClient
        {
            connectionId = int.Parse(data[0]),
            position = new Vector3(float.Parse(data[1]), float.Parse(data[2]), float.Parse(data[3])),
            velocity = new Vector3(float.Parse(data[4]), float.Parse(data[5]), float.Parse(data[6])),
            rotation = new Quaternion(float.Parse(data[7]), float.Parse(data[8]), float.Parse(data[9]), float.Parse(data[10])),
        };
    }
}