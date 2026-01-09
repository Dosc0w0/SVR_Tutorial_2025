// V1

/* 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TextureManager : MonoBehaviour
{

    [Header("Assign your 6 Materials here")]
    [SerializeField] private Material[] materials = new Material[6];

    private int currentIndex = 0;
    private Renderer rend;

    void Awake()
    {
        // Cache the Renderer
        rend = GetComponent<Renderer>();

        // Start by showing just the first material
        rend.materials = new[] { materials[currentIndex] };
    }

    
    public void OnSwitchMaterial(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            ShowNextMaterial();
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame (step)
    void Update()
    {
        // Example using the old Input Manager:
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))    // or your custom button name
        {
            ShowNextMaterial();
        }

        // If you’re using the new Input System, remove this Update block
        // and instead wire up OnSwitchMaterial() below via a PlayerInput component.
    }

    // Call this to cycle forward
    public void ShowNextMaterial()
    {
        currentIndex = (currentIndex + 1) % materials.Length;
        rend.materials = new[] { materials[currentIndex] };
    }

    // --- NEW INPUT SYSTEM HANDLER (optional) ---
    // If you have a PlayerInput component and an action called "SwitchMaterial",
    // it can call this method automatically:
}
*/

// V2 (button holding)

/*
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VRMaterialCycler_XR : MonoBehaviour
{
    [SerializeField] private Material[] materials = new Material[6];
    private int currentIndex = 0;
    private Renderer rend;

    // We'll cache the right-hand controller here
    private InputDevice rightController;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        rend.materials = new[] { materials[currentIndex] };
        InitializeRightController();
    }

    // Finds and stores your Quest's right-hand device
    void InitializeRightController()
    {
        var desired = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(desired, devices);

        if (devices.Count > 0)
            rightController = devices[0];
        else
            Debug.LogWarning("Couldn't find right-hand controller");
    }

    void Update()
    {
        if (!rightController.isValid)
            InitializeRightController();

        // Try to read the trigger as a boolean (pressed past ~0.1)
        if (rightController.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed) && pressed)
        {
            CycleMaterial();
            // Wait until release so we don't cycle every frame while held
            // You could implement your own “debounce” here if you want
        }
    }

    private void CycleMaterial()
    {
        currentIndex = (currentIndex + 1) % materials.Length;
        rend.materials = new[] { materials[currentIndex] };
    }
}
*/

// V3 (button holding)

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VRMaterialCycler_XR : MonoBehaviour
{
    [SerializeField] private Material[] materials = new Material[6];
    [Tooltip("Minimum time (sec) between swaps to avoid jitter")]
    [SerializeField] private float cooldown = 0.2f;

    private int currentIndex = 0;
    private Renderer rend;
    private InputDevice rightController;

    // State for edge detection & cooldown
    private bool triggerHeld = false;
    private float lastSwapTime = -Mathf.Infinity;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        // Show first material
        rend.materials = new[] { materials[currentIndex] };
        InitializeRightController();
    }

    void InitializeRightController()
    {
        var desired = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(desired, devices);

        if (devices.Count > 0)
            rightController = devices[0];
        else
            Debug.LogWarning("VRMaterialCycler_XR: No right-hand controller found");
    }

    void Update()
    {
        // Re-find controller if it gets disconnected
        if (!rightController.isValid)
            InitializeRightController();

        // Read the trigger as a float (0.0 to 1.0)
        if (rightController.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            // Rising edge: value crosses threshold and cooldown elapsed
            if (!triggerHeld && triggerValue >= 0.8f && Time.time - lastSwapTime >= cooldown)
            {
                triggerHeld = true;
                lastSwapTime = Time.time;
                CycleMaterial();
            }
            // Falling edge: release beyond lower threshold
            else if (triggerHeld && triggerValue <= 0.2f)
            {
                triggerHeld = false;
            }
        }
    }

    private void CycleMaterial()
    {
        currentIndex = (currentIndex + 1) % materials.Length;
        rend.materials = new[] { materials[currentIndex] };
    }
}
