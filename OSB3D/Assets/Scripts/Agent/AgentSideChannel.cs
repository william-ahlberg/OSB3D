using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;
using System.Text;
using System;


public class AgentInfoSideChannel : SideChannel
{
    public AgentInfoSideChannel()
    {
        ChannelId = new Guid("621f0a70-4f87-11ea-a6bf-784f4387d1f7");
    }

    protected override void OnMessageReceived(IncomingMessage msg)
    {
        Debug.Log(msg.ToString()); 
    }

    public void SendAgentInfo()
    {



        using (var msgOut = new OutgoingMessage())
        {
            msgOut.WriteString("");
            QueueMessageToSend(msgOut);



        }


    }
}
