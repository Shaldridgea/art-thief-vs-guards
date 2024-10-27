using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DoorwayController : MonoBehaviour
{
    [System.Serializable]
    public class DoorData
    {
        public string name;
        public Transform doorPivot;
        public NavMeshObstacle doorObstacle;
        [HideInInspector]
        public float startAngle;
        [HideInInspector]
        public int triggerCount;
    }

    [SerializeField]
    private DoorData[] targetDoors;

    [SerializeField]
    private OcclusionPortal doorwayPortal;

    int openDoorCount;

    void Start()
    {
        foreach(var d in targetDoors)
        {
            d.startAngle = d.doorPivot.eulerAngles.y;
            if(d.doorObstacle)
                d.doorObstacle.enabled = false;
        }
    }

    public void AgentEnter(string doorOrigin)
    {
        // If an agent has entered then a door is always going to
        // be open, so our occlusion portal should also be open
        if(doorwayPortal != null)
            doorwayPortal.open = true;

        foreach(var d in targetDoors)
        {
            if(d.name == doorOrigin)
            {
                // If the door was not open before, count it as opened
                if (d.triggerCount == 0)
                    ++openDoorCount;

                ++d.triggerCount;
                if(d.doorObstacle)
                    d.doorObstacle.enabled = true;
                break;
            }
        }
    }

    public void AgentExit(string doorOrigin)
    {
        foreach (var d in targetDoors)
        {
            if (d.name == doorOrigin)
            {
                --d.triggerCount;
                // If our door is going to close
                if (d.triggerCount == 0)
                {
                    if (d.doorObstacle)
                    {
                        d.doorObstacle.enabled = false;
                        d.doorObstacle.carving = false;
                    }
                    --openDoorCount;
                }
                break;
            }
        }
    }

    public bool IsDoorBeingUsed(string target)
    {
        foreach (var d in targetDoors)
        {
            if (d.name == target)
                return d.triggerCount > 0;
        }
        return false;
    }

    public void SwingDoor(string target, float swingAngle)
    {
        foreach (var d in targetDoors)
        {
            if (d.name == target)
            {
                LeanTween.cancel(d.doorPivot.gameObject);
                var tween = LeanTween.rotateY(d.doorPivot.gameObject, d.startAngle + swingAngle, 0.3f);

                if (swingAngle != 0f && d.doorObstacle != null)
                    tween.setOnComplete(() => d.doorObstacle.carving = true);
                else if(swingAngle == 0f)
                {
                    // If our occlusion portal should be closed we
                    // do that when all our doors are fully closed
                    tween.setOnComplete(ClosePortalWhenDoorsClose);
                }
                break;
            }
        }
    }

    private void ClosePortalWhenDoorsClose()
    {
        if (doorwayPortal == null)
            return;

        if (openDoorCount == 0)
            doorwayPortal.open = false;
    }

    private void OnValidate()
    {
        if (targetDoors == null)
            return;

        foreach(var d in targetDoors)
        {
            if (d.doorPivot != null && d.doorObstacle == null)
                d.doorObstacle = d.doorPivot.GetComponentInChildren<NavMeshObstacle>();
        }
    }
}
