using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeTrashSpawner : MonoBehaviour
{

    [SerializeField] private GameObject tree = null;
    [SerializeField] private GameObject trashCan = null;
    private GameObject[] treeList = new GameObject[10];
    private GameObject[] trashCanList = new GameObject[5];

    // Spawn trees and trash at random positions in its given grid
    void Start()
    {
        SpawnTrees();
        SpawnTrashCans();
        GetComponent<SquirrelSpawner>().SpawnSquirrels();
    }

    // Randomly spawn 10 non overlapping trees
    private void SpawnTrees()
    {
        for (int i = 0; i < 5; i++)
        {
            var newTree1 = Instantiate(tree);
            newTree1.transform.position = new Vector3(10*i + 5 + Random.Range(-2.5f, 2.5f), 0, Random.Range(5f, 32.5f));
            newTree1.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 359f), 0);
            treeList[2*i] = newTree1;

            var newTree2 = Instantiate(tree);
            newTree2.transform.position = new Vector3(10*i + 5 + Random.Range(-2.5f, 2.5f), 0, Random.Range(37.5f, 65f));
            newTree2.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 359f), 0);
            treeList[2*i+1] = newTree2;
        }
    }

    // Randomly spawn 5 non overlapping trash cans
    private void SpawnTrashCans()
    {
        for (int i = 0; i < 5; i++)
        {
            var newTrashCan = Instantiate(trashCan);
            while (true)
            {
                var tempPosition = new Vector3(10*i + 5 + Random.Range(-2.5f, 2.5f), 0, Random.Range(5f, 65f));
                if (Vector3.Distance(tempPosition, treeList[2*i].transform.position) > 7.5f &&
                Vector3.Distance(tempPosition, treeList[2*i+1].transform.position) > 7.5f)
                {
                    newTrashCan.transform.position = tempPosition + new Vector3(0, 0.6f, 0);
                    trashCanList[i] = newTrashCan;
                    break;

                }
            }
        }
    }

    // Getter for treeList
    public GameObject[] getTreeList()
    {
        return treeList;
    }

    // Getter for trashCanList
    public GameObject[] getTrashCanList()
    {
        return trashCanList;
    }
}
