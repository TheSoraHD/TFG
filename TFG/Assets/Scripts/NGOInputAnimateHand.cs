using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NGOInputAnimateHand : NetworkBehaviour
{
    public InputActionProperty gripAnimationAction;
    public InputActionProperty pinchAnimationAction;

    private Animator handAnimator;
    private float gripValue, triggerValue;
    void Start()
    {
        handAnimator = gameObject.GetComponent<Animator>();
    }
    void Update()
    {
        if (IsOwner)
        {
            UpdateAnimationActions();
        }
    }
    private void UpdateAnimationActions()
    {
        triggerValue = pinchAnimationAction.action.ReadValue<float>();
        handAnimator.SetFloat("Trigger", triggerValue);
        Debug.Log(triggerValue);

        gripValue = gripAnimationAction.action.ReadValue<float>();
        handAnimator.SetFloat("Grip", gripValue);
    }
}
