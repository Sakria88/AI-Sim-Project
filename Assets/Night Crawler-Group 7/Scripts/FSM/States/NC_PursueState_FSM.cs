using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//This class is for the purse state-the purse state goes after the enemy tank
public class NC_PursueState_FSM : NC_Base
{


    // create a private varible for the tank(calling an instance of the Night Crawler tank )
    private NightCrawler NCTank;

    public NC_PursueState(NightCrawler NCTank)
    {
        this.NCTank = NCTank;
      
    
    }

    public override Type StateEnter()
    {
        NCTank.stats["PurseState"] = true; //When the State is entered it is running
        return null;
    }

    public override Type StateExit()
    {
        NCTank.stats["PurseState"] = false; //When the state
        return null;
    }

    public override Type StateUpdate()
    {
        NCTank.PursueState();


        return null;
    }
}