using System.Collections;
using UnityEngine;

public class ActionFlee : GoapAction
{
    public ActionFlee() {
        preconditions.Add("playerInRange", true);
        postconditions.Add("playerInRange", false);
    }
}