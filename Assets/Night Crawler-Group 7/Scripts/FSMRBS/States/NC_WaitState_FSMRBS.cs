using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class NC_WaitState_FSMRBS : NC_BaseState_FSMRBS
{
    private NC_SmartTank_FSMRBS nC_SmartTank_FSMRBS;
    private float waitTime = 3f; // Time to wait in seconds
    private float timer = 0f;
    public NC_WaitState_FSMRBS(NC_SmartTank_FSMRBS tank)
    {
        this.nC_SmartTank_FSMRBS = tank;
    }
    public override Type StateEnter()
    {
        Debug.Log("Entering the wait state FSM");
        timer = 0f; // Reset timer on entering the state
        nC_SmartTank_FSMRBS.TankStop(); // Stop the tank
        nC_SmartTank_FSMRBS.stats["NC_WaitState_FSMRBS"] = true;

        return null;
    }
    public override Type StateUpdate()
    {
        nC_SmartTank_FSMRBS.UpdateGlobalStats(); // Update global stats while waiting
        nC_SmartTank_FSMRBS.CheckWaitTimerExceeded(3f);

        timer += Time.deltaTime; // Increment timer by the time elapsed since last frame

        //Check the rules to see if there are any that need to be used

        foreach (Rule item in nC_SmartTank_FSMRBS.rules.GetRules)
        {
            if (item.CheckRule(nC_SmartTank_FSMRBS.stats) != null)
            {
                return item.CheckRule(nC_SmartTank_FSMRBS.stats);
            }
        }

        return null; // Stay in the wait state
    }
    public override Type StateExit()
    {
        Debug.Log("Exiting the wait state");
        nC_SmartTank_FSMRBS.stats["NC_WaitState_FSMRBS"] = false;

        nC_SmartTank_FSMRBS.TankGo(); // Resume tank movement on exiting the state
        return null;
    }
}