using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    [Header("INPUT")]
    [SerializeField] private InputAction movement;  // controls for moving
    [SerializeField] private InputAction turning;   // controls for looking
    [SerializeField] private InputAction pause;     // pauses game
    [SerializeField] private InputAction interact;  // interacts with item
    [SerializeField] private CharacterController controller = null;
    [SerializeField] private Camera cam = null;

    [Header("STATS")]
    [SerializeField] public float speed = 3.5f;
    [SerializeField] public float mouseSens;
    [SerializeField] public float gravityValue = -9.81f;
    [SerializeField] public float distanceToGround = 0.5f;
    [SerializeField] public float camRotation = 0;

    [Header("UI ELEMENTS")]
    [SerializeField] Text dialogBox;
    [SerializeField] Text pauseBox;
    [SerializeField] public LayerMask GroundLayer;
    [SerializeField] private bool isPaused;
    [SerializeField] GameObject myEventSystem;
    [SerializeField] public Slider SensitivitySlider;
    [SerializeField] private bool sawTheLight;

    private float dialogClearTimer;
    private bool setFirstButton = false;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main;
        interact.performed += _ => Interact();
        pause.performed += _ => Pause();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        myEventSystem = GameObject.Find("EventSystem");
        mouseSens = 10f;
    }

    void Update()
    {
        if(PauseManager.paused)
        {
            FreeMouse();
            if (setFirstButton == false)
            {
                myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(GameObject.Find("Slider"));
                setFirstButton = true;
            }
            mouseSens = SensitivitySlider.value;
            return;
        }
        if(PauseManager.paused == false)
        {
            LockMouse();
            setFirstButton = false;
        }
        Move();
        Watching();
        Turn();
    }
    public void Pause()
    {
        if (isPaused == false)
        {
            PauseGame();
        }
        else
            ResumeGame();

    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0;
        pauseBox.text = "PAUSED";

    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseBox.text = "";
        Time.timeScale = 1;
    }

    //if player presses space, do this.
    private void Interact()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 8f))
        {
            if (hit.collider.tag.Equals("flashlight"))
            {
                FreeMouse();
                SceneManager.LoadScene("WinScreen");
            }
        }

    }
    
    private void Watching()
    {
        RaycastHit hit;   

        if (sawTheLight == false)
        {
            dialogClearTimer += Time.deltaTime;
            if (dialogClearTimer >= 5f)
            {
                dialogBox.text = "";
            }
            
        }

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 5f) && hit.collider.tag.Equals("flashlight"))
        {
            Debug.DrawRay(cam.transform.position, cam.transform.forward, Color.blue);
            dialogBox.text = "Interact with to leave.";
            sawTheLight = true;
            dialogClearTimer = 0;
            return;
        }
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 150f) && hit.collider.tag.Equals("flashlight"))
        {
            Debug.DrawRay(cam.transform.position, cam.transform.forward, Color.blue);
            dialogBox.text = "The source of light...";
            dialogClearTimer = 0;
            sawTheLight = true;
        }
        else
        {
            sawTheLight = false;
        }
            
    }

    private IEnumerator ClearDialog(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            dialogBox.text = "";
        }
    }
    private void Move()
    {
        float x = movement.ReadValue<Vector2>().x;
        float z = movement.ReadValue<Vector2>().y;
        //reconverts the axis to work on the XZ plane
        Vector3 direction = transform.right * x + transform.forward * z;
        Vector3 fall = Vector3.up;
        //print("pog");

        if (OnGround() == false)
        {
            //playerVelocity.y += gravityValue * Time.deltaTime;
            controller.Move(fall * Time.deltaTime * gravityValue);
        }
        controller.Move(direction * Time.deltaTime * speed);
    }

    public bool OnGround()
    {
        bool r = false;

        Vector3 origin = transform.position + (Vector3.up * distanceToGround);
        Vector3 dir = -Vector3.up;
        float dis = distanceToGround + 0.1f;
        RaycastHit hit;

        Debug.DrawRay(origin, dir * distanceToGround); //Debug Line to Show Raycasting Distance

        //If the Raycast is hitting the ground return true and alter the player's position to stay above ground.
        if (Physics.Raycast(origin, dir, out hit, dis, GroundLayer))
        {
            r = true;
            Vector3 targetPosition = hit.point;
            transform.position = targetPosition;
        }

        return r;
    }
    private void Turn()
    {
        float mouseX = turning.ReadValue<Vector2>().x * mouseSens * Time.deltaTime;
        float mouseY = turning.ReadValue<Vector2>().y * mouseSens * Time.deltaTime;

        camRotation -= mouseY;
        camRotation = Mathf.Clamp(camRotation, -90, 90); //clamps mouse
        cam.transform.localRotation = Quaternion.Euler(camRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }


    //literally ends the game
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("enemybody"))
        {
            FreeMouse();
            SceneManager.LoadScene("GameOver");
        }
    }

    private void FreeMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        movement.Enable();
        pause.Enable();
        turning.Enable();
        interact.Enable();
    }
    private void OnDisable()
    {
        movement.Disable();
        pause.Disable();
        turning.Disable();
        interact.Disable();
    }
}
