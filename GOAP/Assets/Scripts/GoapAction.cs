using System.Collections;
using System.Collections.Generic;

public abstract class GoapAction
{
    protected Dictionary<string, bool> preconditions;
    protected Dictionary<string, bool> postconditions;

    // Initialize GoapAction's pre and post conditions
    public GoapAction() {
        preconditions = new Dictionary<string, bool>();
        postconditions = new Dictionary<string, bool>();
    }

    // Returns true if all preconditions are met otherwise false
    public bool CheckPreConditions(Dictionary<string, bool> worldState)
    {
        foreach (string condition in preconditions.Keys)
        {
            if (worldState[condition] != preconditions[condition])
            {
                return false;
            }
        }
        return true;
    }

    // Returns a copy of the new world state once post conditions take place
    public Dictionary<string, bool> SetPostConditions(Dictionary<string, bool> worldState)
    {
        var newState = new Dictionary<string, bool>(worldState);
        foreach (string condition in postconditions.Keys)
        {
            newState[condition] = postconditions[condition];
        }
        return newState;
    }
}