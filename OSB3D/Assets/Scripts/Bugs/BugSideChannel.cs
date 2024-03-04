using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;
using System.Text;
using System;

public class BugSideChannel : SideChannel
{
    Hashtable m_BugParameters = new Hashtable();

    private Dictionary<string, Type> configurationType = new Dictionary<string, Type>
    {
        {"gadget", typeof(int)},
        {"state", typeof(int)},
        {"geometry", typeof(int)},
        {"physics", typeof(int)},
        {"logic", typeof(int)},
    };


    private Dictionary<Type, int> caseType = new Dictionary<Type, int>
    {
        {typeof(bool), 0},
        {typeof(int), 1},
        {typeof(float), 2},
        {typeof(string), 3},
        {typeof(List<string>), 4},
    };

    public BugSideChannel()
    {
        ChannelId = new Guid("b1961881-7cec-498d-9f45-1f7d8a299378");
    }

    protected override void OnMessageReceived(IncomingMessage msg)
    {
        var key = msg.ReadString();

        var messageType = configurationType[key];
        var messageCaseType = caseType[messageType];

        switch (messageCaseType)
        {
            case 0:
                m_BugParameters.Add(key, msg.ReadBoolean());
                break;
            case 1:
                m_BugParameters.Add(key, msg.ReadInt32());
                break;
            case 2:
                m_BugParameters.Add(key, msg.ReadFloat32());
                break;
            case 3:
                m_BugParameters.Add(key, msg.ReadString());
                break;
            case 4:
                int len = msg.ReadInt32();
                string[] value = new string[len];

                for (var i = 0; i < len; i++)
                {
                    value[i] = msg.ReadString();
                }
                m_BugParameters.Add(key, value);
                break;
        }
    }

    public T GetWithDefault<T>(string key, T defaultValue)
    {
        if (m_BugParameters.ContainsKey(key))
        {
            var value = (T)m_BugParameters[key];
            return value;
        }
        else
        {
            return defaultValue;
        }
    }


}




