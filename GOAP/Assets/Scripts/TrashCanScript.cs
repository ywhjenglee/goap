using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCanScript : MonoBehaviour
{
    private bool full = false;
    private bool hasSquirrel = false;
    private GameObject squirrel = null;
    private Renderer rend = null;

    // Make it possible for the trash can to change states every 10 seconds
    void Start()
    {
        rend = gameObject.GetComponent<Renderer>();
        InvokeRepeating("ChangeState", 0, 10);
    }

    // Change the state of the trash can
    private void ChangeState()
    {
        // If there is a squirrel don't change state
        if (hasSquirrel)
        {
            return;
        }
        // Randomly set the state of the trash can
        if (Random.Range(0f, 1f) < 0.5)
        {
            SetFull();
        }
        else
        {
            SetEmpty();
        }
    }

    // Put a squirrel in the trash can
    public void SetSquirrel(GameObject s)
    {
        squirrel = s;
        hasSquirrel = true;
        rend.material.color = new Color32(185, 85, 55, 255);
    }

    // Remove the squirrel from the trash can
    public void RemoveSquirrel()
    {
        squirrel = null;
        hasSquirrel = false;
        rend.material.color = new Color32(85, 85, 85, 255);
    }

    // Getter for full
    public bool GetFull()
    {
        return full;
    }

    // Getter for hasSquirrel
    public bool GetHasSquirrel()
    {
        return hasSquirrel;
    }

    // Set the trash can to full state
    public void SetFull()
    {
        full = true;
        rend.material.color = new Color32(180, 35, 35, 255);
    }

    // Set the trash can to empty state
    public void SetEmpty()
    {
        full = false;
        rend.material.color = new Color32(85, 85, 85, 255);
    }
}
