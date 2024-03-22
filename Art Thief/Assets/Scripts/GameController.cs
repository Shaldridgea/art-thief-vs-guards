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
    private TextMeshProUGUI spyWonText;

    [SerializeField]
    private TextMeshProUGUI guardsWonText;

    [SerializeField]
    private GameObject panel;

    [SerializeField]
    private Button startButton;

    [SerializeField]
    private Transform exitGoal;

    public Vector3 ExitPosition => exitGoal.position;

    [SerializeField]
    private GameObject chooseButtonPrefab;

    [SerializeField]
    private Transform buttonLayout;

    public Transform ArtGoal { get; private set; }

    [SerializeField]
    private List<GuardAgent> guards;

    public List<GuardAgent> Guards => guards;
    
    [SerializeField]
    private ThiefAgent spy;

    public Blackboard GlobalBlackboard { get; private set; }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield break;
        spyWonText.enabled = false;
        guardsWonText.enabled = false;
        // Turn agents off so they don't start working till we want them to
        spy.gameObject.SetActive(false);
        foreach (GuardAgent g in guards)
            g.gameObject.SetActive(false);
        startButton.interactable = false;

        // Wait a frame before showing the painting options, since when reloading the scene for some reason
        // the textures won't be set properly
        yield return new WaitForEndOfFrame();
        startButton.onClick.AddListener(StartGame);
    }

    // Update is called once per frame
    void Update()
    {
        // Speed up the game
        if (Input.GetKey(KeyCode.Space))
            Time.timeScale = 8f;
        else
            Time.timeScale = 1f;

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
        panel.SetActive(false);
        spy.gameObject.SetActive(true);
        foreach (GuardAgent g in guards)
            g.gameObject.SetActive(true);
    }

    public void SpyWon()
    {
        spyWonText.enabled = true;
    }

    public void GuardsWon()
    {
        guardsWonText.enabled = true;
    }

    public void SetArtGoal(Transform goal) { ArtGoal = goal; startButton.interactable = true; }
}
