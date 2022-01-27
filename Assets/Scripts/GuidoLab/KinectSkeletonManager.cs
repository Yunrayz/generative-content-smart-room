using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class KinectSkeletonManager : MonoBehaviour
{
    public StringEvent HandStateRight, HandStateLeft;

    public StringEvent GestureState;

    public bool activateHandCloseEvents = false;
    public bool activateGestureEvents = false;
    public bool activatePlayerGestures = false;
    public bool keepStartingPosition = false;
    public Vector3Bool activeTrackAxis;
    public PlayerToFollow playerIdentifier = PlayerToFollow.Closest_player;

    public PlayerPositiondDict trackingSchema = new PlayerPositiondDict();

    private static int numActiveTracker = 0;
    private Skeleton skelPosition;

    private bool leftHandState;
    private bool rightHandState;

    private Vector3 standardizedFloorSize;
    private Vector3 sensorDisallinment;
    public BoxArea interactiveArea;

    private bool _activeCrouch = false;
    private bool _activeHandRaise = false;
    private bool _activeJump = false;
    private bool _activeHandsForward = false;

    private void Start()
    {
        if (MagicRoomManager.instance != null)
        {
            MagicRoomManager.instance.MagicRoomKinectV2Manager.Skeletons += ManageSkeleton;
            numActiveTracker++;
            if (numActiveTracker == 1)
            {
                MagicRoomManager.instance.MagicRoomKinectV2Manager.StartStreamingSkeletons(250);
            }
            if (MagicRoomManager.instance.systemConfiguration != null)
            {
                sensorDisallinment = new Vector3(MagicRoomManager.instance.systemConfiguration.floorOffsetX, MagicRoomManager.instance.systemConfiguration.floorOffsetY, 0);
                standardizedFloorSize = new Vector3(MagicRoomManager.instance.systemConfiguration.floorSizeX / Mathf.Pow(2.74f, 2), 1, MagicRoomManager.instance.systemConfiguration.floorSizeY / Mathf.Pow(2.88f, 2));
            }
        }
        HandStateRight = new StringEvent();
        HandStateLeft = new StringEvent();
    }

    /// <summary>
    /// Apply scale operations to the raw position
    /// </summary>
    /// <param name="position"></param>
    /// <returns name="Scaled Position">Scaled Position</returns>
    private Vector3 FixPosition(Vector3 position, bool applyAxisFilter = false)
    {
        Vector3 axis = new Vector3(Convert.ToSingle(activeTrackAxis.X), Convert.ToSingle(activeTrackAxis.Y), Convert.ToSingle(activeTrackAxis.Z));
        //rescale according to the standard room size
        //1) Remove the misallignemnt due to the misplacement of the sensor in the physical room
        position = position - sensorDisallinment;
        //2) rescale the position in the standard measurement
        position = Vector3.Scale(position, standardizedFloorSize);
        //3) Transform into the coordiantes in the game
        position = Vector3.Scale(position, interactiveArea.getScaleVector()) + interactiveArea.getOrinig();
        position = Vector3.Scale(position, new Vector3(-1, 1, -1));
        //4) Apply filters for the axis
        // position = Vector3.Scale(position, axisFilter != null ? (Vector3)axisFilter : new Vector3(1, 1, 1));
        if (applyAxisFilter) position = Vector3.Scale(position, axis);
        //Invert a Verctor3bool


        //5) return the computed position
        // Debug.Log("Position: " + position);
        return position;
    }

    private void Update()
    {
        if (skelPosition != null)
        {
            Vector3 invertedAxis = new Vector3(Convert.ToSingle(!activeTrackAxis.X), Convert.ToSingle(!activeTrackAxis.Y), Convert.ToSingle(!activeTrackAxis.Z));
            foreach (var p in trackingSchema)
            {
                Vector3 newposition;
                switch (p.Key)
                {
                    case PartToTrack.BodyOverallPosition: newposition = skelPosition.SpineBase; break;
                    case PartToTrack.Head: newposition = skelPosition.Head; break;
                    case PartToTrack.LeftAnkle: newposition = skelPosition.AnkleLeft; break;
                    case PartToTrack.RightAnkle: newposition = skelPosition.AnkleRight; break;
                    case PartToTrack.LeftElbow: newposition = skelPosition.ElbowLeft; break;
                    case PartToTrack.RightElbow: newposition = skelPosition.ElbowRight; break;
                    case PartToTrack.LeftFoot: newposition = skelPosition.FootLeft; break;
                    case PartToTrack.RightFoot: newposition = skelPosition.FootRight; break;
                    case PartToTrack.LeftHand: newposition = skelPosition.HandLeft; break;
                    case PartToTrack.RightHand: newposition = skelPosition.HandRight; break;
                    case PartToTrack.LeftHip: newposition = skelPosition.HipLeft; break;
                    case PartToTrack.RightHip: newposition = skelPosition.HipRight; break;
                    case PartToTrack.RightShoulder: newposition = skelPosition.ShoulderRight; break;
                    case PartToTrack.LeftShoulder: newposition = skelPosition.ShoulderRight; break;
                    case PartToTrack.LeftWrist: newposition = skelPosition.WristLeft; break;
                    case PartToTrack.RightWrist: newposition = skelPosition.WristRight; break;
                    case PartToTrack.SpineBase: newposition = skelPosition.SpineBase; break;
                    case PartToTrack.SpineMid: newposition = skelPosition.SpineMid; break;
                    case PartToTrack.Neck: newposition = skelPosition.Neck; break;
                    case PartToTrack.KneeLeft: newposition = skelPosition.KneeLeft; break;
                    case PartToTrack.KneeRight: newposition = skelPosition.KneeRight; break;
                    case PartToTrack.SpineShoulder: newposition = skelPosition.SpineShoulder; break;
                    case PartToTrack.HandTipLeft: newposition = skelPosition.HandTipLeft; break;
                    case PartToTrack.ThumbLeft: newposition = skelPosition.ThumbLeft; break;
                    case PartToTrack.HandTipRight: newposition = skelPosition.HandTipRight; break;
                    case PartToTrack.ThumbRight: newposition = skelPosition.ThumbRight; break;
                    default: newposition = Vector3.zero; break;
                }

                if (keepStartingPosition)
                {
                    trackingSchema[p.Key].position = Vector3.Scale(trackingSchema[p.Key].position, invertedAxis) + FixPosition(newposition, true);
                }
                else
                {
                    trackingSchema[p.Key].position = FixPosition(newposition, true);
                }

                if (activateHandCloseEvents)
                {
                    //Dictionary<PartToTrack, bool> hand = new Dictionary<PartToTrack, bool>();

                    //bool haschangehappened = false;
                    if (skelPosition.IsLeftHandClosed() && !leftHandState)
                    {
                        //hand.Add(PartToTrack.LeftHand, true);
                        leftHandState = true;
                        //haschangehappened = true;
                        HandStateLeft?.Invoke("LEFTHAND_OPEN");
                    }
                    if (!skelPosition.IsLeftHandClosed() && leftHandState)
                    {
                        //hand.Add(PartToTrack.LeftHand, false);
                        leftHandState = true;
                        //haschangehappened = true;

                        HandStateLeft?.Invoke("LEFTHAND_CLOSE");
                    }
                    if (skelPosition.IsRightHandClosed() && !rightHandState)
                    {
                        //hand.Add(PartToTrack.RightHand, true);
                        rightHandState = true;
                        HandStateRight?.Invoke("RIGHTHAND_OPEN");
                        //haschangehappened = true;
                    }
                    if (!skelPosition.IsRightHandClosed() && rightHandState)
                    {
                        //hand.Add(PartToTrack.RightHand, false);
                        HandStateRight?.Invoke("RIGHTHAND_CLOSE");
                        rightHandState = true;
                        //haschangehappened = true;
                    }
                    /*if (haschangehappened)
                    {
                        HandState?.Invoke(hand);
                    }*/
                }
            }
            if (activatePlayerGestures)
            {

                // Crouch
                Vector3 SpineBase = FixPosition(skelPosition.SpineBase);
                Vector3 KneeLeft = FixPosition(skelPosition.KneeLeft);
                Vector3 KneeRight = FixPosition(skelPosition.KneeRight);
                Vector3 HandLeft = FixPosition(skelPosition.HandLeft);
                Vector3 HandRight = FixPosition(skelPosition.HandRight);
                if ((HandLeft.y < KneeLeft.y || HandRight.y < KneeRight.y))
                {
                    // EventManager.TriggerEvent("OnCrouch", gameObject); // Deprecated, Is called on key press and on key release
                    if (!_activeCrouch)
                    {
                        EventManager.TriggerEvent("OnCrouchStart", gameObject);
                        EventManager.TriggerEvent("OnCrouch", gameObject);
                        _activeCrouch = true;
                    }
                    Debug.Log("Crouch");
                }
                else if (_activeCrouch)
                {
                    EventManager.TriggerEvent("OnCrouchEnd", gameObject);
                    EventManager.TriggerEvent("OnCrouch", gameObject);
                    _activeCrouch = false;
                }

                // HandRaise
                Vector3 Head = FixPosition(skelPosition.Head);
                if ((Head.y < HandLeft.y && Head.y < HandRight.y))
                {
                    if (!_activeHandRaise)
                    {
                        EventManager.TriggerEvent("OnHandRaiseStart", gameObject);
                        EventManager.TriggerEvent("OnHandRaise", gameObject);
                        _activeHandRaise = true;
                    }
                    Debug.Log("HandRaise");
                }
                else if (_activeHandRaise)
                {
                    EventManager.TriggerEvent("OnHandRaiseEnd", gameObject);
                    EventManager.TriggerEvent("OnHandRaise", gameObject);
                    _activeHandRaise = false;
                }

                // HandsForward
                // Vector3 ElbowLeft = FixPosition(skelPosition.ElbowLeft);
                // Vector3 ElbowRight = FixPosition(skelPosition.ElbowRight);
                Vector3 ShoulderLeft = FixPosition(skelPosition.ShoulderLeft);
                Vector3 ShoulderRight = FixPosition(skelPosition.ShoulderRight);
                if ((Math.Abs(ShoulderLeft.y - HandLeft.y) < 0.2 && Math.Abs(ShoulderRight.y - HandRight.y) < 0.2) &&
                (Math.Abs(ShoulderLeft.x - HandLeft.x) < 0.2 && Math.Abs(ShoulderRight.x - HandRight.x) < 0.2))
                {
                    if (!_activeHandsForward)
                    {
                        EventManager.TriggerEvent("OnHandsForwardStart", gameObject);
                        EventManager.TriggerEvent("OnHandsForward", gameObject);
                        _activeHandsForward = true;
                    }
                    Debug.Log("HandsForward");
                }
                else if (_activeHandsForward)
                {
                    EventManager.TriggerEvent("OnHandsForwardEnd", gameObject);
                    EventManager.TriggerEvent("OnHandsForward", gameObject);
                    _activeHandsForward = false;
                }

                // Jump
                Vector3 FootLeft = FixPosition(skelPosition.FootLeft);
                Vector3 FootRight = FixPosition(skelPosition.FootRight);
                if ((FootLeft.y > -0.92f && FootRight.y > -0.92f))
                {
                    if (!_activeJump)
                    {
                        EventManager.TriggerEvent("OnJumpStart", gameObject);
                        EventManager.TriggerEvent("OnJump", gameObject);
                        _activeJump = true;
                    }
                    Debug.Log("Jump");
                }
                else if (_activeJump)
                {
                    EventManager.TriggerEvent("OnJumpEnd", gameObject);
                    EventManager.TriggerEvent("OnJump", gameObject);
                    _activeJump = false;
                }
            }


            if (activateGestureEvents)
            {
                if (skelPosition.Gestures.Length > 0)
                {
                    foreach (string s in skelPosition.Gestures)
                    {
                        GestureState?.Invoke(s);
                    }
                }
            }
        }
    }

    private void ManageSkeleton(Dictionary<ulong, Skeleton> skel)
    {
        if (playerIdentifier != PlayerToFollow.Closest_player)
        {
            Debug.Log(System.Convert.ToUInt64(playerIdentifier));
            foreach (ulong id in skel.Keys)
            {
                if (id == System.Convert.ToUInt64(playerIdentifier))
                {
                    skelPosition = skel[id];
                }
            }
        }
        else
        {
            ulong closestId = skel.Keys.ToArray()[0];
            float distance = Mathf.Infinity;
            foreach (ulong id in skel.Keys)
            {
                if (skel[id].SpineBase.z < distance && skel[id].SpineBase.z > 0)
                {
                    closestId = id;
                    distance = skel[id].SpineBase.z;
                }
            }
            skelPosition = skel[closestId];
        }
    }

    private void OnDestroy()
    {
        if (MagicRoomManager.instance != null)
        {
            if (numActiveTracker > 0)
            {
                MagicRoomManager.instance.MagicRoomKinectV2Manager.Skeletons -= ManageSkeleton;
                numActiveTracker--;
                if (numActiveTracker == 0)
                {
                    MagicRoomManager.instance.MagicRoomKinectV2Manager.StopStreamingSkeletons();
                }
            }
        }
    }

}