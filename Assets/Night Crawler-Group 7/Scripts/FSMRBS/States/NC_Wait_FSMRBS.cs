using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class NC_Wait_FSMRBS : NC_BaseState_FSMRBS
{
    private NC_SmartTank_FSMRBS nC_SmartTank_FSMRBS;
    private float waitTime = 3f; // Time to wait in seconds
    private float timer = 0f;
    public NC_Wait_FSMRBS(NC_SmartTank_FSMRBS tank)
    {
        this.nC_SmartTank_FSMRBS = tank;
    }
    public override Type StateEnter()
    {
        Debug.Log("Entering the wait state FSM");
        timer = 0f; // Reset timer on entering the state
        nC_SmartTank_FSMRBS.TankStop(); // Stop the tank
        return null;
    }
    public override Type StateUpdate()
    {
        timer += Time.deltaTime; // Increment timer by the time elapsed since last frame
        if (timer >= waitTime)
        {
            return typeof(NC_PatrolState_FSMRBS); // After waiting, switch to patrol state
        }
        return null; // Stay in the wait state
    }
    public override Type StateExit()
    {
        Debug.Log("Exiting the wait state");
        nC_SmartTank_FSMRBS.TankGo(); // Resume tank movement on exiting the state
        return null;
    }
}