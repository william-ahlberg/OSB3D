using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using Random = UnityEngine.Random;

public class LogicBug : BugBase
{

    private GameObject button1;
    private GameObject button2;
    private GameObject button3;
    private GameObject[] buttons;


    // Start is called before the first frame update
    private void Start()
    {
        base.Start();
        button1 = GameObject.Find("1");
        button2 = GameObject.Find("2");
        button3 = GameObject.Find("3");
        ChangeLogic();



    }

    private void Update()
    {
        
    
    
    
    }

    private void ChangeLogic()
    {
        button1.name = "3";
        button2.name = "2";
        button3.name = "1";




    }







}




