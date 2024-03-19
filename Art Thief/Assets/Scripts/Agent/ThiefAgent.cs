using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.AI;

public class ThiefAgent : Agent
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Exit"))
        {
            // Declare the spy the winner if they get to the exit
            // This ignores whether the spy has stolen the art or not since the guard's win condition is catching the spy
            GameController.Instance.SpyWon();
        }
    }
}