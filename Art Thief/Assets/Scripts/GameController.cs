﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    private void Awake()
    {
        // Set singleton instance
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        GlobalBlackboard = new Blackboard();
    }

    public Transform ArtGoal { get; set; }

    [SerializeField]
    private List<GuardAgent> guards;

    public List<GuardAgent> Guards => guards;
    
    [SerializeField]
    private ThiefAgent thief;

    public Blackboard GlobalBlackboard { get; private set; }

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Reload the whole scene
        if(Input.GetKeyDown(KeyCode.R))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadSceneAsync("gallery");
        }
    }

    public void StartGame(int startingGuardCount, Transform artGoal)
    {
        Level.Instance.Thief.ActivateAgent();
        Level.Instance.Thief.ArtGoal = artGoal;
        for (int i = 0; i < startingGuardCount; ++i)
        {
            GuardAgent guard = Level.Instance.GuardList[i];
            guard.ActivateAgent();
        }
    }
}
