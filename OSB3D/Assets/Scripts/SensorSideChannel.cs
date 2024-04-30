using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;
using System.Text;
using System;


public class SensorSideChannel : SideChannel
{
    Hashtable m_SensorParameters = new Hashtable();

    private Dictionary<string, Type> configurationType = new Dictionary<string, Type>
    {
        {"continuous_actions", typeof(int)},
        {"available_actions", typeof(List<string>)},
        {"position_type", typeof(string)},
        {"include_bug_position", typeof(bool)},
        {"grayscale", typeof(bool)},
        {"camera_resolution_width", typeof(int)},
        {"camera_resolution_height", typeof(int)},
        {"ray_perception_plane", typeof(int)},
        {"ray_perception_cone", typeof(int)},
        {"ray_perception_slice", typeof(int)},
        {"semantic_map_x", typeof(int)},
        {"semantic_map_y", typeof(int)},
        {"semantic_map_z", typeof(int)},
        {"number_of_gadget", typeof(int)},
        {"number_of_state", typeof(int) },
        {"number_of_geometry", typeof(int) },
        {"number_of_physics", typeof(int) },
        {"number_of_logic", typeof(int) },
        {"vector_obs_settings", typeof(bool) },
        {"camera_settings", typeof(bool) },
        {"ray_perception_settings", typeof(bool) },
        {"semantic_map_settings", typeof(bool) },


    };


    private Dictionary<Type, int> caseType = new Dictionary<Type, int>
    {
        {typeof(bool), 0},
        {typeof(int), 1},
        {typeof(float), 2},
        {typeof(string), 3},
        {typeof(List<string>), 4},
    };

    public SensorSideChannel()
    {
        ChannelId = new Guid("aa97d987-4c42-4878-b597-3de40edf66a6");
    }

    protected override void OnMessageReceived(IncomingMessage msg)
    {
        var key = msg.ReadString();
 
        var messageType = configurationType[key];
        var messageCaseType = caseType[messageType];
            
        switch (messageCaseType)
        {
            case 0:
                m_SensorParameters.Add(key, msg.ReadBoolean());
                break;
            case 1:
                m_SensorParameters.Add(key, msg.ReadInt32());
                break;
            case 2:
                m_SensorParameters.Add(key, msg.ReadFloat32());
                break;
            case 3:
                m_SensorParameters.Add(key, msg.ReadString());
                break;
            case 4:
                int len = msg.ReadInt32();
                string[] value = new string[len];

                for (var i = 0; i < len; i++)
                {
                    value[i] = msg.ReadString();
                }
                m_SensorParameters.Add(key, value);
                break;
        }
    }

    public T GetWithDefault<T>(string key, T defaultValue)
    {
        if (m_SensorParameters.ContainsKey(key))
        {
            var value = (T)m_SensorParameters[key];
            return value;
        }
        else
        {
            return defaultValue;
        }
    }


}




