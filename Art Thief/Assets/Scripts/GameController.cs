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
    private CameraControl gameCamera;

    [SerializeField]
    private SimulationHUD menuHUD;

    [SerializeField]
    private StartScreen startScreen;

    public Blackboard GlobalBlackboard { get; private set; }

    // Start is called before the first frame update
    private void Start()
    {
        gameCamera.enabled = false;
        menuHUD.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Reload the whole scene
        if(Input.GetKeyDown(KeyCode.R))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadSceneAsync(0);
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
        gameCamera.transform.LookAt(Level.Instance.Thief.transform);
        gameCamera.enabled = true;
        menuHUD.gameObject.SetActive(true);
    }

    public void EndGame(Consts.Team winner)
    {
        Level.Instance.Thief.DeactivateAgent();
        for (int i = 0; i < Level.Instance.GuardList.Count; ++i)
        {
            GuardAgent guard = Level.Instance.GuardList[i];
            guard.DeactivateAgent();
        }

        menuHUD.ShowWinnerText(winner);
    }
}
