using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//This class is for the purse state-the purse state goes after the enemy tank
public class NC_PursueState_FSM : NC_BaseScript_FSM
{


    // create a private varible for the tank(calling an instance of the Enemy Night Crawler tank )
    private NC_SmartTank_FSM NCEnTank;

    public NC_PursueState_FSM(NC_SmartTank_FSM NCEnTank)
    {
        this.NCEnTank = NCEnTank;
      
    
    }

    public override Type StateEnter()
    {
        NCEnTank.stats["PurseState"] = true; //When the State is entered it is running
        return null;
    }

    public override Type StateUpdate()
    {

        NCEnTank.PursueState();


        return null;
    }

    public override Type StateExit()
    {
        NCEnTank.stats["PurseState"] = false; //When the state
        return null;
    }
}