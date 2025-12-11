using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Rules
{
    //This class that adds the Rules into a list

    public void AddRule(Rules rule)
    {

        GetRules.Add(rule);
    }

    public List<Rule> GetRules { get; } = new List<Rule>();



}