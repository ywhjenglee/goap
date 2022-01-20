using System.Collections;
using UnityEngine;

public class ActionIdleLook : GoapAction
{
    public ActionIdleLook() {
        preconditions.Add("playerInRange", false);
        postconditions.Add("nuts", true);
        postconditions.Add("trashCans", true);
    }
}