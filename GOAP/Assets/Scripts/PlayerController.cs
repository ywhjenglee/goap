using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{

    [SerializeField] Transform playerCamera = null;
    [SerializeField] CharacterController playerBody = null;
    [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] float movementSpeed = 8f;
    [SerializeField] private GameObject ghostText = null;
    [SerializeField] private GameObject nut = null;
    private float cameraPitch = 0f;
    private float gravity = -9.81f;
    private float velocityY;
    private bool ghostMode = true;

    // Lock cursor and make it invisible
    void Start()
    {
        playerBody = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update First Person Player
    void Update()
    {
        GhostMode();
        PlayerLook();
        PlayerMovement();
    }

    // Ghost-mode controls
    private void GhostMode()
    {
        // On clicking space-bar
        if (Input.GetButtonDown("Jump"))
        {
            ghostMode = !ghostMode;
        }
        if (ghostMode) {
            // Set visibility of ghost-mode text and deactivate player nav mesh obstacle
            GetComponent<NavMeshObstacle>().enabled = false;
            ghostText.gameObject.SetActive(true);

            // On clicking left mouse button
            if (Input.GetButtonDown("Fire1"))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                // Shoot a ray and detect if a game object has been hit
                if (Physics.Raycast(ray, out hit, 10))
                {
                    // If terrain is hit place nut
                    if (hit.transform.gameObject.tag == "Terrain")
                    {
                        var newNut = Instantiate(nut);
                        newNut.transform.position = hit.point + new Vector3(0, 0.15f, 0);
                    }
                    // If nut is hit destroy it
                    else if (hit.transform.gameObject.tag == "Nut")
                    {
                        Destroy(hit.transform.gameObject);
                    }
                    // If trash is hit
                    else if (hit.transform.gameObject.tag == "Trash")
                    {
                        var t = hit.transform.gameObject.GetComponent<TrashCanScript>();
                        // If trash does not contain a squirrel
                        if (!t.GetHasSquirrel())
                        {
                            // Fill if empty
                            if (t.GetFull())
                            {
                                t.SetEmpty();
                            }
                            // Empty if full
                            else
                            {
                                t.SetFull();
                            }
                        }
                    }
                }
            }
        }
        else {
            // Remove visibility of ghost-mode text and activate player nav mesh obstacle
            GetComponent<NavMeshObstacle>().enabled = true;
            ghostText.gameObject.SetActive(false);
        }
    }

    // Update Player's camera
    private void PlayerLook()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        cameraPitch -= mouseDelta.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);
        playerCamera.localEulerAngles = Vector3.right * cameraPitch;
        transform.Rotate(Vector3.up * mouseDelta.x * mouseSensitivity);
    }

    // Update Player's position
    private void PlayerMovement()
    {
        Vector3 inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        inputDirection.Normalize();

        if (playerBody.isGrounded)
        {
            velocityY = 0f;
        }
        velocityY += gravity * Time.deltaTime;

        Vector3 playerVelocity = (transform.right * inputDirection.x + transform.forward * inputDirection.z) * movementSpeed + Vector3.up * velocityY;
        playerBody.Move(playerVelocity * Time.deltaTime);
    }

    // Getter for ghostMode
    public bool GetGhostMode()
    {
        return ghostMode;
    }
}
