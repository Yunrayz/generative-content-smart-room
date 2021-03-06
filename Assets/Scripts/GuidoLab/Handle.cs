using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Handle : MonoBehaviour
{
    public bool onCrouch;
    public bool onStep;
    public bool onJump;
    public bool isGrabbable = true;
    public bool isGrabbed = false;

    [Tooltip("If zero, the bond won't break")]
    public float breakDistance = 3f;

    [Tooltip("Guide a second player to the other handle, if zero, no guide will be shown")]
    public float guidePlayerAfter = 10f;
    private GameObject _grabbedBy;
    private List<GameObject> playerInsideTrigger = new List<GameObject>();
    private List<GameObject> playerNotAllowed = new List<GameObject>(); //List of players that have grabbed another brother handle

    void Start()
    {
        if (transform.parent == null) Debug.LogWarning("Handle MUST have a parent to work correctly");
        EventManager.StartListening("OnCrouch", OnCrouchHandler);
        EventManager.StartListening("OnJump", OnJumpHandler);
        EventManager.StartListening("HandleGrabbed", onHandleGrabbed);
        EventManager.StartListening("HandleUngrabbed", onHandleUngrabbed);
    }
    void OnDestroy()
    {
        EventManager.StopListening("OnCrouch", OnCrouchHandler);
        EventManager.StopListening("OnJump", OnJumpHandler);
        EventManager.StopListening("HandleGrabbed", onHandleGrabbed);
        EventManager.StopListening("HandleUngrabbed", onHandleUngrabbed);
    }
    private void Update()
    {
        if (breakDistance != 0 && isGrabbed)
        {
            if (Vector3.Distance(transform.position, _grabbedBy.transform.position) > breakDistance)
            {
                Ungrab(_grabbedBy);
            }
        }

    }
    //Make the handle Grabbed
    void Grab(GameObject sender)
    {
        Debug.Log(gameObject + " grabbed");
        EventManager.TriggerEvent("HandleGrabbed", gameObject, new EventDict() { ["player"] = sender, ["parent"] = transform.parent });
        _grabbedBy = sender;
        gameObject.GetComponent<ChaseWithRigidBody>().target = sender.tag.StartsWith("Player") ? sender.GetComponent<Player>().target : sender.transform;
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        isGrabbable = false;
        isGrabbed = true;
        playerInsideTrigger.RemoveAll(item => item);

        CancelGuidePlayer();

        BroadcastMessage("OnHelperGlowDisable", gameObject, SendMessageOptions.DontRequireReceiver);
        // if (GetComponent<HelperGlow>() != null) //Case when the handle has the outline
        // {
        //     GetComponent<HelperGlow>().enabled = false;
        // }
        if (GetComponentInParent<ObjectStateHandler>()) //Case where this handle deactivate the outline outside the gameObject
        {
            GetComponentInParent<ObjectStateHandler>().SendMessage("OnGrabbed", gameObject);
        }

    }
    void Ungrab(GameObject ungrabFrom, bool askedFromBrother = false)
    {
        Debug.Log(gameObject + " ungrabbed");
        if (!isGrabbed)
        {
            CancelGuidePlayer();
            playerNotAllowed.RemoveAll(item => item);
            return;
        }
        if (!askedFromBrother)
        {
            EventManager.TriggerEvent("HandleUngrabbed", gameObject, new EventDict() { ["parent"] = transform.parent });
        }
        else
        {
            BroadcastMessage("onHandleUngrabbed", new EventDict() { ["sender"] = gameObject });
        }
        _grabbedBy = null;
        gameObject.GetComponent<ChaseWithRigidBody>().target = null;
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        Invoke("MakeKinematic", 1f);
        CancelGuidePlayer();
        isGrabbable = true;
        isGrabbed = false;
        playerNotAllowed.RemoveAll(item => item);
        BroadcastMessage("OnHelperGlowEnable", gameObject, SendMessageOptions.DontRequireReceiver);
    }
    void OnCrouchHandler(EventDict data)
    {
        GameObject sender = (GameObject)data["sender"];
        //Two cases:
        //is going to be grabbed
        if (playerInsideTrigger.Contains(sender) && !playerNotAllowed.Contains(sender) && onCrouch && !isGrabbed)
        {
            Grab(sender);
        }
        // //is going to be ungrabbed
        // else if (onCrouch && isGrabbed)
        // {
        //     Ungrab(sender);
        // }
    }
    void OnJumpHandler(EventDict data)
    {
        GameObject sender = (GameObject)data["sender"];

        if (playerInsideTrigger.Contains(sender) && !playerNotAllowed.Contains(sender) && onJump)
        {
            Grab(sender);
        }
    }
    void onHandleGrabbed(EventDict dict)
    {
        if (isGrabbed) return; //If is grabbed, don't want to stuck

        if ((GameObject)dict["sender"] == gameObject) return; //Check if message is from myself

        if ((Transform)dict["parent"] != transform.parent) return; //Check if message is not from my brother

        //If I'm here. I'm the other handle
        //I don't want to be grabbed by the same player
        playerNotAllowed.Add((GameObject)dict["player"]);
        //Stuck myself for 2P interaction
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        if (guidePlayerAfter != 0)
        {
            GuidePlayer(guidePlayerAfter);
        }
    }
    void onHandleUngrabbed(EventDict dict)
    {
        if ((GameObject)dict["sender"] == gameObject) return; //Return if message is from myself

        if ((Transform)dict["parent"] != transform.parent) return; //Return if message is not from my brother

        //If I'm here. I'm the other handle
        //I ungrab too
        Ungrab(_grabbedBy, true);
    }

    void MakeKinematic()
    {
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
    }
    void GuidePlayer(float delay)
    {
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (playerNotAllowed.Contains(player)) continue;
            GuideLineManager.instance?.CreateLine(player.transform, gameObject.transform, delay);
            break;
        }
    }

    void CancelGuidePlayer()
    {
        // GuideLineManager.DeleteLine(gameObject.transform, end: gameObject.transform);
        EventManager.TriggerEvent("DeleteGuideLine", gameObject);
    }


    void OnTriggerEnter(Collider other)
    {
        if (isGrabbable)
        {
            if (other.gameObject.tag.StartsWith("Player"))
            {
                playerInsideTrigger.Add(other.gameObject);

                if (onStep && !playerNotAllowed.Contains(other.gameObject))
                {
                    Grab(other.gameObject);
                }

            }
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (isGrabbable)
        {
            if (other.gameObject.tag.StartsWith("Player"))
            {
                playerInsideTrigger.Remove(other.gameObject);
            }
        }
    }
}
