using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DoorController : MonoBehaviour
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

    // Start is called before the first frame update
    void Start()
    {
        foreach(var d in targetDoors)
        {
            d.startAngle = d.doorPivot.eulerAngles.y;
            if(d.doorObstacle)
                d.doorObstacle.enabled = false;
        }
    }

    public void AgentEnter(string target)
    {
        foreach(var d in targetDoors)
        {
            if(d.name == target)
            {
                ++d.triggerCount;
                if(d.doorObstacle)
                    d.doorObstacle.enabled = true;
                break;
            }
        }
    }

    public void AgentExit(string target)
    {
        foreach (var d in targetDoors)
        {
            if (d.name == target)
            {
                --d.triggerCount;
                if (d.triggerCount == 0)
                    if (d.doorObstacle)
                    {
                        d.doorObstacle.enabled = false;
                        d.doorObstacle.carving = false;
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
                if (swingAngle != 0f)
                    tween.setOnComplete(() => d.doorObstacle.carving = true);
                break;
            }
        }
    }

    private void OnValidate()
    {
        foreach(var d in targetDoors)
        {
            if (d.doorPivot != null && d.doorObstacle == null)
                d.doorObstacle = d.doorPivot.GetComponentInChildren<NavMeshObstacle>();
        }
    }
}
