using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{

    public static GameInput Instance { get; private set; }

    PlayerControls playerControls;

    public event EventHandler OnInteractPerformed;

    private void Awake()
    {
        Instance = this;

        playerControls = new PlayerControls();
        playerControls.Enable();
        playerControls.PlayerActions.Interact.performed += Interact_performed;
    }

    private void Interact_performed(InputAction.CallbackContext obj)
    {
        OnInteractPerformed?.Invoke(this, EventArgs.Empty);
    }
}
