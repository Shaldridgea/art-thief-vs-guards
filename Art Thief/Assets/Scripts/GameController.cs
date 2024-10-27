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

    [SerializeField]
    private CameraControl cameraController;

    public CameraControl CameraController => cameraController;

    [SerializeField]
    private Camera gameCamera;

    public Camera GameCamera => gameCamera;

    [SerializeField]
    private SimulationHUD menuHUD;

    [SerializeField]
    private StartScreen startScreen;

    [SerializeField]
    [Range(30, 120)]
    private int framerateCap = 60;

    public Blackboard GlobalBlackboard { get; private set; }

    private void Start()
    {
        cameraController.enabled = false;
        menuHUD.gameObject.SetActive(false);
        Application.targetFrameRate = framerateCap;
    }

    void Update()
    {
        // Reload the whole scene
        if(Input.GetKeyDown(KeyCode.R))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 1f;
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
        cameraController.transform.LookAt(Level.Instance.Thief.transform);
        cameraController.enabled = true;
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
