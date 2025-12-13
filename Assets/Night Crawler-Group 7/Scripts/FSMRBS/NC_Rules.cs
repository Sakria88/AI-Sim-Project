using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Class <c>Rules</c> is used to store a list of Rule objects
/// </summary>
public class Rules
{
    /// <summary>
    /// Method <c>AddRule</c> adds a Rule to the list of rules
    /// </summary>
    /// <param name="rule"></param>
    public void AddRule(Rule rule)
    {
        GetRules.Add(rule);
    }

    public List<Rule> GetRules { get; } = new List<Rule>(); // List to store Rule objects
}