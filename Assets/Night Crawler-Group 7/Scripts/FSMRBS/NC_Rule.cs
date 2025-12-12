using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Rule
{
    //Antecedents are facts for the rules to check
    public string antecedentA;
    public string antecedentB;

    //The consequent is the new rule that that runs if the facts passes
    public Type consequentState;

    //Predicate is the check for the rules
    public Predicate compare;
    public enum Predicate
    { And, Or, nAnd}

    //The constructor sets how the rule needs to be written
    public Rule(string antecedentA, string antecedentB, Type consequentState, Predicate compare)
    {
        //
        this.antecedentA = antecedentA;
        this.antecedentB = antecedentB;
        this.consequentState = consequentState;
        this.compare = compare;
    }

    public Type CheckRule(Dictionary<string, bool> stats)
    {
        //setting the string varibles to booleans for the statement
        bool antecedentABool = stats[antecedentA];
        bool antecedentBBool = stats[antecedentB];

        switch (compare)
        {

            //The And case means both facts have to be true to return the consequent
            case Predicate.And:
                if(antecedentABool && antecedentBBool)
                {
                    return consequentState;
                }
                else
                {
                    return null;
                }

            //The Or case means only one of the facts need to be true for the consquent to be returned
            case Predicate.Or:
                if (antecedentABool || antecedentBBool)
                {
                    return consequentState;
                }
                else
                {
                    return null;
                }

            //The Not means both have to be false for the consequent to be rerturned
            case Predicate.nAnd:
                if(!antecedentABool && !antecedentBBool)
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