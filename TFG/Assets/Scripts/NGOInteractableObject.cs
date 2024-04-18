using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEditor.Experimental.GraphView;
using Valve.VR.InteractionSystem;
using UnityEngine.Events;
using static Valve.VR.InteractionSystem.Hand;

[RequireComponent(typeof(Rigidbody))]
public class NGOInteractableObject : NetworkBehaviour
{
    [Tooltip("The flags used to attach this object to the hand.")]
    public NGOFallbackHand.AttachmentFlags attachmentFlags = NGOFallbackHand.AttachmentFlags.ParentToHand | NGOFallbackHand.AttachmentFlags.VelocityMovement;

    [Tooltip("When detaching the object, should it return to its original parent?")]
    public bool restoreOriginalParent = false;

    protected bool attached = false; //TO-DO: Attach to several hands
    protected Vector3 attachPosition;
    protected Quaternion attachRotation;

    protected RigidbodyInterpolation hadInterpolation = RigidbodyInterpolation.None;

    protected new Rigidbody rigidbody;

    [System.NonSerialized]
    public NGOFallbackHand attachedToHand;


    public delegate void OnAttachedToHandDelegate(NGOFallbackHand hand);
    public delegate void OnDetachedFromHandDelegate(NGOFallbackHand hand);

    public event OnAttachedToHandDelegate onAttachedToHand;
    public event OnDetachedFromHandDelegate onDetachedFromHand;



    [System.NonSerialized]
    public List<NGOFallbackHand> hoveringHands = new List<NGOFallbackHand>();
    public NGOFallbackHand hoveringHand
    {
        get
        {
            if (hoveringHands.Count > 0)
                return hoveringHands[0];
            return null;
        }
    }


    public bool isDestroying { get; protected set; }
    public bool isHovering { get; protected set; }
    public bool wasHovering { get; protected set; }




    protected virtual void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.maxAngularVelocity = 50.0f;
    }

    protected virtual void OnHandHoverBegin(NGOFallbackHand hand)
    {
        wasHovering = isHovering;
        isHovering = true;

        hoveringHands.Add(hand);
    }

    protected virtual void OnHandHoverEnd(NGOFallbackHand hand)
    {
        wasHovering = isHovering;

        hoveringHands.Remove(hand);

        if (hoveringHands.Count == 0)
            isHovering = false;
    }

    protected virtual void HandHoverUpdate(NGOFallbackHand hand)
    {
        GrabTypes startingGrabType = hand.GetGrabStarting();

        if (startingGrabType != GrabTypes.None) {
            hand.AttachObject(gameObject, startingGrabType, attachmentFlags, null);
            hand.HideGrabHint();
        }
    }

    protected virtual void OnAttachedToHand(NGOFallbackHand hand)
    {
        //Debug.Log("<b>[SteamVR Interaction]</b> Pickup: " + hand.GetGrabStarting().ToString());

        hadInterpolation = this.rigidbody.interpolation;

        if (onAttachedToHand != null)
            onAttachedToHand.Invoke(hand);

        attached = true;
        attachedToHand = hand;

        hand.HoverLock(null);

        rigidbody.interpolation = RigidbodyInterpolation.None;

        attachPosition = transform.position;
        attachRotation = transform.rotation;

    }


    //-------------------------------------------------
    protected virtual void OnDetachedFromHand(NGOFallbackHand hand)
    {
        if (onDetachedFromHand != null)
            onDetachedFromHand.Invoke(hand);

        attached = false;
        attachedToHand = null;

        hand.HoverUnlock(null);

        rigidbody.interpolation = hadInterpolation;
    }

    protected virtual void HandAttachedUpdate(NGOFallbackHand hand)
    {
        if (hand.IsGrabEnding(this.gameObject)){
            hand.DetachObject(gameObject, restoreOriginalParent);
        }
    }

}
