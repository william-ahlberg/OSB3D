using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;
using System.Text;
using System;

public class ActionSideChannel : SideChannel
{
    Hashtable m_ActionParameters = new Hashtable();

    private Dictionary<string, Type> configurationType = new Dictionary<string, Type>
    {
        {"action_space_settings", typeof(bool)},
        {"continuous_actions", typeof(bool)},
        {"available_actions", typeof(List<string>)},
    };


    private Dictionary<Type, int> caseType = new Dictionary<Type, int>
    {
        {typeof(bool), 0},
        {typeof(int), 1},
        {typeof(float), 2},
        {typeof(string), 3},
        {typeof(List<string>), 4},
    };

    public ActionSideChannel()
    {
        ChannelId = new Guid("4a6982f9-d298-4f7b-b7eb-bb7012603bba");
    }

    protected override void OnMessageReceived(IncomingMessage msg)
    {
        var key = msg.ReadString();

        var messageType = configurationType[key];
        var messageCaseType = caseType[messageType];

        switch (messageCaseType)
        {
            case 0:
                m_ActionParameters.Add(key, msg.ReadBoolean());
                break;
            case 1:
                m_ActionParameters.Add(key, msg.ReadInt32());
                break;
            case 2:
                m_ActionParameters.Add(key, msg.ReadFloat32());
                break;
            case 3:
                m_ActionParameters.Add(key, msg.ReadString());
                break;
            case 4:
                int len = msg.ReadInt32();
                string[] value = new string[len];

                for (var i = 0; i < len; i++)
                {
                    value[i] = msg.ReadString();
                }
                m_ActionParameters.Add(key, value);
                break;
        }
    }

    public T GetWithDefault<T>(string key, T defaultValue)
    {
        if (m_ActionParameters.ContainsKey(key))
        {
            var value = (T)m_ActionParameters[key];
            return value;
        }
        else
        {
            return defaultValue;
        }
    }


}




