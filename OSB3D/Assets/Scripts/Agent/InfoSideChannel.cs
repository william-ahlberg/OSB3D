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

    public InfoSideChannel()
    {
        ChannelId = new Guid("a0b3abca-2146-4ddb-ac7b-713aebedd67f");
    }

    private void Update()
    { 
        
    
    }
    
    protected override void OnMessageReceived(IncomingMessage msg)
    {
    


    
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

    /*public void SendTestInfo()
    {
        string testMessage = "envSize";
        using (var msgOut = new OutgoingMessage())
        {
            msgOut.WriteString(testMessage);
             
            QueueMessageToSend(msgOut);
        
        
        }
    
    }
    */


}


