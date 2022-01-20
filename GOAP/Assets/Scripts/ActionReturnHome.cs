using System.Collections;
using UnityEngine;

public class ActionReturnHome : GoapAction
{
    public ActionReturnHome() {
        preconditions.Add("playerInRange", false);
        preconditions.Add("holding", true);
        postconditions.Add("nuts", false);
        postconditions.Add("trashCans", false);
        postconditions.Add("holding", false);
    }
}