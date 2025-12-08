using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NC_PatrolState_FSM : NC_Base
{
    //This is the patrol state for the night crawler enemy
    private NightCrawler _nightCrawler;
    public NC_PatrolState(NightCrawler nightCrawler)
    {
        _nightCrawler = nightCrawler;
    }
    public override Type StateEnter()
    {
        //Code to execute when entering the patrol state
        Debug.Log("Entering Patrol State");
        // Set patrol destination or initialize patrol behavior here
        return null;
    }
    public override Type StateUpdate()
    {
        //Code to execute every frame while in the patrol state
        Debug.Log("Updating Patrol State");
        // Implement patrol logic here, such as moving along waypoints
        // Example condition to switch to another state
        
        return null; // Remain in the current state
    }
    public override Type StateExit()
    {
        //Code to execute when exiting the patrol state
        Debug.Log("Exiting Patrol State");
        // Clean up or reset variables related to patrol behavior here
        return null;
    }
}