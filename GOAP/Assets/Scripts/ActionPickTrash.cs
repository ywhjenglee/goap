using System.Collections;
using UnityEngine;

public class ActionPickTrash : GoapAction
{
    public ActionPickTrash() {
        preconditions.Add("playerInRange", false);
        preconditions.Add("trashCans", true);
        preconditions.Add("holding", false);
        postconditions.Add("holding", true);
    }
}