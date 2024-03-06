using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputAnimateHand : MonoBehaviour
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
        triggerValue = pinchAnimationAction.action.ReadValue<float>();
        handAnimator.SetFloat("Trigger", triggerValue);

        gripValue = gripAnimationAction.action.ReadValue<float>();
        handAnimator.SetFloat("Grip", gripValue);
    }
}
