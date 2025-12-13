using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Class <c>Rule</c> is used to create rules for the FSM to follow
/// </summary>
public class Rule
{
    //Antecedents are facts for the rules to check
    public string antecedentA;
    public string antecedentB;
    public string antecedentC;

    //The consequent is the new rule that that runs if the facts passes
    public Type consequentState;

    //Predicate is the check for the rules
    public Predicate compare;
    public enum Predicate
    { And, Or, nAnd }

    //The constructor sets how the rule needs to be written
    public Rule(string antecedentA, string antecedentB, string antecedentC, Type consequentState, Predicate compare)
    {
        //
        this.antecedentA = antecedentA;
        this.antecedentB = antecedentB;
        this.antecedentC = antecedentC;
        this.consequentState = consequentState;
        this.compare = compare;
    }

    /// <summary>
    /// Checks the rule against the stats provided 
    /// </summary>
    /// <param name="stats"></param>
    /// <returns></returns>
    public Type CheckRule(Dictionary<string, bool> stats)
    {
        //setting the string varibles to booleans for the statement
        bool antecedentABool = stats[antecedentA];
        bool antecedentBBool = stats[antecedentB];
        bool antecedentCBool = stats[antecedentC];

        switch (compare)
        {

            //The And case means both facts have to be true to return the consequent
            case Predicate.And:
                if (antecedentABool && antecedentBBool && antecedentCBool)
                {
                    return consequentState;
                }
                else
                {
                    return null;
                }

            //The Or case means only one of the facts need to be true for the consquent to be returned
            case Predicate.Or:
                if (antecedentABool || antecedentBBool || antecedentCBool)
                {
                    return consequentState;
                }
                else
                {
                    return null;
                }

            //The Not means both have to be false for the consequent to be rerturned
            case Predicate.nAnd:
                if (!antecedentABool && !antecedentBBool && !antecedentCBool)
                {
                    return consequentState;
                }
                else
                {
                    return null;
                }

            default:
                return null;
        }
    }

}