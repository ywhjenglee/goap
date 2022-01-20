using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquirrelSpawner : MonoBehaviour
{
    [SerializeField] private GameObject squirrel;
    private GameObject[] treeList;

    // Randomly spawn 5 squirrels with different home trees
    public void SpawnSquirrels()
    {
        treeList = gameObject.GetComponent<TreeTrashSpawner>().getTreeList();
        int i = 0;
        while (i < 5)
        {
            int rand = Random.Range(0, 10);
            if (treeList[rand].GetComponent<TreeScript>().GetHomeSquirrel() == null)
            {
                var newSquirrel = Instantiate(squirrel);
                treeList[rand].GetComponent<TreeScript>().SetHomeSquirrel(newSquirrel);
                newSquirrel.GetComponent<SquirrelScript>().SetHomeTree(treeList[rand]);
                newSquirrel.transform.position = treeList[rand].transform.position + new Vector3(0,0,2);
                newSquirrel.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 359f), 0);
                i++;
            }
        }
    }
}
