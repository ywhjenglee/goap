using System.Collections;
using UnityEngine;

public class ActionPickNuts : GoapAction
{
    public ActionPickNuts() {
        preconditions.Add("playerInRange", false);
        preconditions.Add("nuts", true);
        preconditions.Add("holding", false);
        postconditions.Add("holding", true);
    }
}