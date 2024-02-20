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

    public SensorSideChannel()
    {
        ChannelId = new Guid("aa97d987-4c42-4878-b597-3de40edf66a6");
    }

    protected override void OnMessageReceived(IncomingMessage msg)
    {

        var receivedString = msg.ReadString();
        Debug.Log("From Python: " + receivedString);


    }

    public void GetWithDefault<T>(string key, T defaultValue)
    { 
     
        //Func<T> valueOut;

        //bool hasKey = m_Parameters.TryGetValue(key, out valueOut);
        //return haskey ? valueOut.Invoke() : defaultValue; 
    
        

    }
    // Start is called before the first frame update
    void Start()
    {
            


    }

    // Update is called once per frame
    void Update()
    {
    
    
    
    
    }
}
