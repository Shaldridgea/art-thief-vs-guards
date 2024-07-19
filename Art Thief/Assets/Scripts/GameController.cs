using System.Collections;
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

    public void StartGame()
    {
        // Start game by activating the AI agents and removing the UI
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        thief.gameObject.SetActive(true);
        foreach (GuardAgent g in guards)
            g.gameObject.SetActive(true);
    }
}
