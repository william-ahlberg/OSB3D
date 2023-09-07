using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugBase : MonoBehaviour
{
    
    private Vector3 _position;
    private int _id;
    private string _type;
    private int _stateDepth;
    private bool _isActive;

    public int Id
    {
        get
        {
            return _id;
        }
        set
        {
        
        
        
        
        }
    
    
    }

    public Vector3 Position
    {
        get
        {
            return _position;
        }
        set
        {
            
        
        
        
        }
    
    
    }

    public string Type
    {
        get
        {
            return _type;
        }
        set
        {
        
        
        
        
        }
    
    
    }

    private void Start()
    {
    
    

    }

    private void Update()
    {
    
    
    
    
    }  
    
    

    protected virtual void BugTrigger()
    {
    
    
    
    }
    
    protected virtual void BugBehavior()
    {
        
    
    }

    protected virtual void BugVisual()
    {
    
        
    
    
    }
    
    protected virtual void BugPlace()
    {
    
    
    
    }




}
