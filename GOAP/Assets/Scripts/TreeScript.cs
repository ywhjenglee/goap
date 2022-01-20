using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeScript : MonoBehaviour
{

    [SerializeField] private GameObject nut = null;
    private int nuts = 0;
    private GameObject[,] nutList = new GameObject[3,3];
    private GameObject homeSquirrel = null;
    private bool isOccupied = false;
    
    // Spawn nuts at 2 second intervals
    void Start()
    {
        InvokeRepeating("CountNuts", 0, 2);
        InvokeRepeating("SpawnNut", 0, 2); 
    }

    // Count the amount of nuts within the trees radius
    private void CountNuts()
    {
        var count = 0;
        foreach (GameObject nut in GameObject.FindGameObjectsWithTag("Nut"))
        {
            float dist = Vector3.Distance(transform.position, nut.transform.position);
            if (dist < 4)
            {
                count++;
            }
        }
        nuts = count;
    }

    // Spawn a nut at a random position under the tree if there is less than 5 nuts
    private void SpawnNut()
    {
        if (nuts < 5)
        {
            while (true)
            {
                var newNut = Instantiate(nut);
                var tempX = Random.Range(0, 3);
                var tempZ = Random.Range(0, 3);

                if (nutList[tempX,tempZ] == null && !(tempX == 1 && tempZ == 1))
                {
                    nutList[tempX,tempZ] = newNut;
                    newNut.transform.position = transform.position + new Vector3(1.75f*(tempX-1), 0.15f, 1.75f*(tempZ-1));
                    break;
                }
            }
        }
    }

    // Getter for homeSquirrel
    public GameObject GetHomeSquirrel()
    {
        return homeSquirrel;
    }

    // Setter for homeSquirrel
    public void SetHomeSquirrel(GameObject squirrel)
    {
        homeSquirrel = squirrel;
    }

    // Getter for isOccupied
    public bool GetIsOccupied()
    {
        return isOccupied;
    }

    // Make tree occupied
    public void SetSquirrel()
    {
        isOccupied = true;
    }

    // Make tree unoccupied
    public void RemoveSquirrel()
    {
        isOccupied = false;
    }
}
