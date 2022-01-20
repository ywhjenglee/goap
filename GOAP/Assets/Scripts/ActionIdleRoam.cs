using System.Collections;
using UnityEngine;

public class ActionIdleRoam : GoapAction
{
    public ActionIdleRoam() {
        preconditions.Add("playerInRange", false);
        postconditions.Add("nuts", true);
        postconditions.Add("trashCans", true);
    }
}