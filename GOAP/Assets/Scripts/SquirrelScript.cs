using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SquirrelScript : MonoBehaviour
{

    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private TextMesh text;
    private Transform player;
    private PlayerController controller;
    private GameObject[] treeList;
    private GameObject[] trashCanList;
    private Transform homeTree = null;
    private Queue<GameObject> nuts = new Queue<GameObject>();
    private Queue<GameObject> trashCans = new Queue<GameObject>();
    private Transform closestTree = null;
    private int holding = 0;
    private bool fleeing = false;
    private bool inTrash = false;
    private Dictionary<string, bool> worldState = new Dictionary<string, bool>();
    private List<GoapAction> allActions = new List<GoapAction>();
    private Queue<GoapAction> plan = new Queue<GoapAction>();

    void Start()
    {
        // Initialize fields
        SetInitialState();
        SetAllActions();
        player = GameObject.Find("First Person Player").GetComponent<Transform>();
        controller = GameObject.Find("First Person Player").GetComponent<PlayerController>();
        treeList = GameObject.Find("Terrain").GetComponent<TreeTrashSpawner>().getTreeList();
        trashCanList = GameObject.Find("Terrain").GetComponent<TreeTrashSpawner>().getTrashCanList();
        // Get an initial plan and follow it
        GetPlan(worldState);
        StartCoroutine(FollowPlan());
    }

    void Update()
    {
        // Make text face player
        text.transform.LookAt(Camera.main.transform);
        text.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        // If player not in ghost mode
        if (!controller.GetGhostMode())
        {
            // If squirrel is not currently fleeing and a player is in range
            if (!fleeing && !inTrash && Vector3.Distance(transform.position, player.position) < 5)
            {
                // Interrupt action and replan
                worldState["playerInRange"] = true;
                StopAllCoroutines();
                GetPlan(worldState);
                StartCoroutine(FollowPlan());
            }
        }
        // Find for closest tree
        float closest = float.PositiveInfinity;
        foreach (GameObject tree in treeList)
        {
            float dist = Vector3.Distance(transform.position, tree.transform.position);
            if (dist < closest)
            {
                closest = dist;
                closestTree = tree.transform;
            }
        }
        // Memorize in FIFO order 2 trash cans which were within range
        foreach (GameObject trashCan in trashCanList)
        {
            float dist = Vector3.Distance(transform.position, trashCan.transform.position);
            if (dist < 5)
            {
                if (trashCans.Count > 1)
                {
                    trashCans.Dequeue();
                }
                trashCans.Enqueue(trashCan);
            }
        }
        // Memorize in FIFO order 5 nuts which were within range
        foreach (GameObject nut in GameObject.FindGameObjectsWithTag("Nut"))
        {
            float dist = Vector3.Distance(transform.position, nut.transform.position);
            if (dist < 5)
            {
                if (nuts.Count > 4)
                {
                    nuts.Dequeue();
                }
                nuts.Enqueue(nut);
            }
        }
    }

    // Set initial world state
    private void SetInitialState()
    {
        worldState.Add("playerInRange", false);
        worldState.Add("nuts", false);
        worldState.Add("trashCans", false);
        worldState.Add("holding", false);
    }

    // Set all possible Goap Actions
    private void SetAllActions()
    {
        allActions.Add(new ActionIdleRoam());
        allActions.Add(new ActionIdleLook());
        allActions.Add(new ActionPickNuts());
        allActions.Add(new ActionPickTrash());
        allActions.Add(new ActionFlee());
        allActions.Add(new ActionReturnHome());
    }

    // Helper function to shuffle a given list (for randomization)
    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var temp = list[i];
            int j = Random.Range(i, list.Count);
            list[i] = list[j];
            list[j] = temp;
        }
    }

    // Get a plan to follow (brute force method but always returns a shortest path)
    private void GetPlan(Dictionary<string, bool> currentState)
    {
        // Clear plan and initialize queue to track potential paths
        plan.Clear();
        Queue<(List<GoapAction>, Dictionary<string, bool>)> path = new Queue<(List<GoapAction>, Dictionary<string, bool>)>();
        // Start the "BFS" search from the root
        path.Enqueue((new List<GoapAction>(), currentState));
        while (path.Count != 0) {
            (var actions, var state) = path.Dequeue();
            // If goal is satisfied return the plan of action
            if (state["holding"] && !state["playerInRange"]) // Goal state
            {
                // Add the action to perform when goal state is reached
                actions.Add(new ActionReturnHome());
                string s = "";
                foreach (GoapAction a in actions)
                {
                    plan.Enqueue(a);
                    if (a.ToString() == "ActionReturnHome")
                    {
                        s = s + a.ToString().Substring(6);
                    }
                    else
                    {
                        s = s + a.ToString().Substring(6) + "->";
                    }
                }
                text.text = s;
                break;
            }
            // If path is too long (should not happen given the low amount of possible actions) reject path
            if (actions.Count > 5)
            {
                continue;
            }
            // Shuffle list of goap actions
            var shuffled = new List<GoapAction>(allActions);
            Shuffle(shuffled);
            // For all possible actions enqueue those of which preconditions were satisfied
            foreach (GoapAction action in shuffled)
            {
                if (action.CheckPreConditions(state))
                {
                    var actionsCopy = new List<GoapAction>(actions);
                    actionsCopy.Add(action);
                    path.Enqueue((actionsCopy, action.SetPostConditions(state)));
                }
            }
        }
    }

    // Follow the plan
    private IEnumerator FollowPlan()
    {
        while (plan.Count != 0)
        {
            // Peek the action to perform
            GoapAction action = plan.Peek();
            // Execute the action
            switch (action.ToString())
            {
                case "ActionIdleRoam": yield return StartCoroutine(IdleRoam()); break;
                case "ActionIdleLook": yield return StartCoroutine(IdleLook()); break;
                case "ActionPickNuts": yield return StartCoroutine(PickNuts()); break;
                case "ActionPickTrash": yield return StartCoroutine(PickTrash()); break;
                case "ActionFlee": yield return StartCoroutine(Flee()); break;
                case "ActionReturnHome": yield return StartCoroutine(ReturnHome()); break;
            }       
        }
    }

    // Idle roam action
    private IEnumerator IdleRoam()
    {
        // Select random destination to roam to
        agent.SetDestination(new Vector3(Random.Range(2f,48f), 0, Random.Range(2f,68f)));
        // Wait until destination is reached
        while (Vector3.Distance(transform.position, agent.destination) > 0.5)
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.25f);
        // If enough nuts and trash cans were found
        if (nuts.Count == 5 && trashCans.Count == 2)
        {
            // Set post conditions and complete action
            worldState = allActions[0].SetPostConditions(worldState);
            plan.Dequeue();
            text.text = text.text.Substring(10);
        }
        else
        {
            // Interrupt action, replan and follow new plan
            StopAllCoroutines();
            GetPlan(worldState);
            StartCoroutine(FollowPlan());
        }
    }

    // Idle look action
    private IEnumerator IdleLook()
    {
        // Play a look animation
        agent.SetDestination(new Vector3(1, 0, 1) + transform.position);
        while (Vector3.Distance(transform.position, agent.destination) > 0.5)
        {
            yield return null;
        }
        agent.SetDestination(new Vector3(-2, 0, 0) + transform.position);
        while (Vector3.Distance(transform.position, agent.destination) > 0.5)
        {
            yield return null;
        }
        agent.SetDestination(new Vector3(0, 0, -2) + transform.position);
        while (Vector3.Distance(transform.position, agent.destination) > 0.5)
        {
            yield return null;
        }
        agent.SetDestination(new Vector3(2, 0, 0) + transform.position);
        while (Vector3.Distance(transform.position, agent.destination) > 0.5)
        {
            yield return null;
        }
        agent.SetDestination(new Vector3(0, 0, 2) + transform.position);
        while (Vector3.Distance(transform.position, agent.destination) > 0.5)
        {
            yield return null;
        }
        // Memorize in FIFO order 2 trash cans which were within look range
        foreach (GameObject trashCan in trashCanList)
        {
            float dist = Vector3.Distance(transform.position, trashCan.transform.position);
            if (dist < 10)
            {
                if (trashCans.Count > 1)
                {
                    trashCans.Dequeue();
                }
                trashCans.Enqueue(trashCan);
            }
        }
        // Memorize in FIFO order 5 nuts which were within look range
        foreach (GameObject nut in GameObject.FindGameObjectsWithTag("Nut"))
        {
            float dist = Vector3.Distance(transform.position, nut.transform.position);
            if (dist < 10)
            {
                if (nuts.Count > 4)
                {
                    nuts.Dequeue();
                }
                nuts.Enqueue(nut);
            }
        }
        // If enough nuts and trash cans were found
        if (nuts.Count == 5 && trashCans.Count == 2)
        {
            // Set post conditions and complete action
            worldState = allActions[1].SetPostConditions(worldState);
            plan.Dequeue();
            text.text = text.text.Substring(10);
        }
        else
        {
            // Interrupt action, replan and follow new plan
            StopAllCoroutines();
            GetPlan(worldState);
            StartCoroutine(FollowPlan());            
        }
    }

    // Pick nuts action
    private IEnumerator PickNuts()
    {
        // Go through entire memory list of nuts or until holding 3 nuts
        int i = 0;
        while (holding < 3)
        {
            if (i > 4)
            {
                break;
            }
            i++;
            GameObject nut = null;
            try
            {
                nut = nuts.Dequeue();
            }
            catch (System.Exception)
            {
                continue;
            }
            if (nut == null)
            {
                continue;
            }
            // Move squirrel to nut position
            agent.SetDestination(nut.transform.position);
            // Wait until destination is reached
            while (Vector3.Distance(transform.position, agent.destination) > 0.5)
            {
                yield return null;
            }
            yield return new WaitForSeconds(0.25f);
            // If nut does not exist anymore continue
            if (nut == null)
            {
                continue;
            }
            // Increment holding and destroy the nut
            holding++;
            Destroy(nut);
        }
        // If holding enough nuts
        if (holding > 2)
        {
            // Set post conditions and complete action
            worldState = allActions[2].SetPostConditions(worldState);
            plan.Dequeue();
            text.text = text.text.Substring(10);
        }
        else
        {
            // Interrupt action, replan and follow new plan
            StopAllCoroutines();
            GetPlan(worldState);
            StartCoroutine(FollowPlan());
        }
    }

    // Pick trash action
    private IEnumerator PickTrash()
    {
        // Look for oldest trash can in memory
        var trashCan = trashCans.Peek();
        var t = trashCan.GetComponent<TrashCanScript>();
        // Move squirrel to trash can position
        agent.SetDestination(trashCan.transform.position);
        // Wait until destination is reached
        while (Vector3.Distance(transform.position, agent.destination) > 2)
        {
            yield return null;
        }
        // If trash can is full
        if (t.GetFull())
        {
            // Make squirrel hold food and empty the trash can
            yield return new WaitForSeconds(0.25f);
            holding = 3;
            t.SetEmpty();
            // Set post conditions and complete action
            worldState = allActions[3].SetPostConditions(worldState);
            plan.Dequeue();
            text.text = text.text.Substring(11);
        }
        // If trash can contains a trapped squirrel
        else if (t.GetHasSquirrel())
        {
            // Interrupt action, replan and follow new plan
            yield return new WaitForSeconds(0.25f);
            StopAllCoroutines();
            GetPlan(worldState);
            StartCoroutine(FollowPlan());
        }
        // If trash can is empty
        else
        {
            // Trap squirrel in the trash can for 2 seconds
            inTrash = true;
            t.SetSquirrel(gameObject);
            gameObject.transform.localScale = new Vector3(0, 0, 0);
            yield return new WaitForSeconds(2);
            gameObject.transform.localScale = new Vector3(1, 1, 1);
            t.RemoveSquirrel();
            inTrash = false;
            yield return new WaitForSeconds(0.25f);
            // Interrupt action, replan and follow new plan
            StopAllCoroutines();
            GetPlan(worldState);
            StartCoroutine(FollowPlan());
        }
    }

    // Flee action
    private IEnumerator Flee()
    {
        fleeing = true;
        // Move squirrel to closest tree
        agent.SetDestination(closestTree.position);
        // Wait until destination is reached
        while (Vector3.Distance(transform.position, agent.destination) > 2)
        {
            yield return null;
        }
        var t = closestTree.GetComponent<TreeScript>();
        // If tree is not occupied
        if (!t.GetIsOccupied())
        {
            // Hide squirrel in the tree for 2 seconds
            t.SetSquirrel();
            gameObject.transform.localScale = new Vector3(0, 0, 0);
            yield return new WaitForSeconds(2);
            // Make sure player is not within range
            while (Vector3.Distance(closestTree.position, player.position) < 5)
            {
                yield return null;
            }
            t.RemoveSquirrel();
            gameObject.transform.localScale = new Vector3(1, 1, 1);
            // Set post conditions and complete action
            worldState = allActions[4].SetPostConditions(worldState);
            plan.Dequeue();
            text.text = text.text.Substring(6);
            fleeing = false;
        }
        // If occupied keep running until out of range
        else
        {
            agent.SetDestination(transform.position+4*(transform.position-player.position));
            while (Vector3.Distance(transform.position, player.position) < 8)
            {
                yield return null;
            }
            // Set post conditions and complete action
            worldState = allActions[4].SetPostConditions(worldState);
            plan.Dequeue();
            text.text = text.text.Substring(6);
            fleeing = false;
        }        
    }

    // Return home action
    private IEnumerator ReturnHome()
    {
        // Move squirrel to home tree
        agent.SetDestination(homeTree.position);
        // Wait until destination is reached
        while (Vector3.Distance(transform.position, agent.destination) > 2)
        {
            yield return null;
        }
        // Drop off nuts/trash
        holding = 0;
        // Set post conditions and complete action
        worldState = allActions[5].SetPostConditions(worldState);
        plan.Dequeue();
        text.text = text.text.Substring(10);
        yield return new WaitForSeconds(1);
        // Make a new plan to follow
        StopAllCoroutines();
        GetPlan(worldState);
        StartCoroutine(FollowPlan());
    }
    
    // Setter for homeTree
    public void SetHomeTree(GameObject tree)
    {
        homeTree = tree.transform;
    }

    // Getter for fleeing
    public bool GetFleeing()
    {
        return fleeing;
    }
}