using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;
using System.Text;
using System;


public class InfoSideChannel : SideChannel
{
    Hashtable m_InfoParameters = new Hashtable();
    IList<float> ISpawnPoint;
    public InfoSideChannel()
    {
        ChannelId = new Guid("a0b3abca-2146-4ddb-ac7b-713aebedd67f");
    }

    private void Update()
    { 
        
    
    }
    
    protected override void OnMessageReceived(IncomingMessage msg)
    {
        var key = msg.ReadString();
        ISpawnPoint = msg.ReadFloatList();
        Vector3 spawnPoint = new Vector3(ISpawnPoint[0], ISpawnPoint[1], ISpawnPoint[2]);
        m_InfoParameters["spawn_point"] =  spawnPoint;
        Debug.Log("Hashtable info" + m_InfoParameters);
    }

    public void SendAgentInfo(Vector3 agentPosition)
    {

        using (var msgOut = new OutgoingMessage())
        {
            msgOut.WriteFloatList(new float[] { agentPosition.x, agentPosition.y, agentPosition.z });
            QueueMessageToSend(msgOut);
        }
        
    }
    public void SendEnvironmentInfo(Vector3 environmentSize)
    {
        using (var msgOut = new OutgoingMessage())
        {
            msgOut.WriteFloatList(new float[] { environmentSize.x, environmentSize.y, environmentSize.z });
             
            
            QueueMessageToSend(msgOut);
        
        
        }
    
    }

    public T GetWithDefault<T>(string key, T defaultValue)
    {
        if (m_InfoParameters.ContainsKey(key))
        {
            var value = (T)m_InfoParameters[key];
            return value;    
        }
        else
        {
            return defaultValue;
        }
    
    
    
    }



}


