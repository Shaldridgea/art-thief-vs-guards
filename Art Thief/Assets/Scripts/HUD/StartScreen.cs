using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.AI;

/// <summary>
/// Start screen UI for setting up parameters for the simulation
/// </summary>
public class StartScreen : MonoBehaviour
{
    public enum OptionFocus
    {
        Steal,
        ThiefStart,
        Guard
    }

    [Header("UI References")]
    [SerializeField]
    private Button leftButton;

    [SerializeField]
    private Button rightButton;

    [SerializeField]
    private Button confirmButton;

    [SerializeField]
    private Button infoButton;

    [SerializeField]
    private GameObject infoScreen;

    [SerializeField]
    private TMP_Dropdown artCategoryDropdown;

    [SerializeField]
    private TMP_Dropdown startTargetDropdown;

    [SerializeField]
    private TMP_InputField guardCountInput;

    [SerializeField]
    private Camera levelViewCamera;

    private OptionFocus currentFocus;

    private int currentGuardCount;

    private int guardCountMax = 4;

    private List<BoxCollider> artFocusList;

    private int artFocusIndex;

    private float artCameraDistance = 2.5f;

    private Transform thiefStartCameraTransform;

    void Start()
    {
        leftButton.onClick.AddListener(LeftButtonPressed);
        rightButton.onClick.AddListener(RightButtonPressed);
        confirmButton.onClick.AddListener(ConfirmButton);
        infoButton.onClick.AddListener(InfoButtonPressed);

        artCategoryDropdown.onValueChanged.AddListener(ArtCategoryChanged);
        startTargetDropdown.onValueChanged.AddListener(ThiefStartTargetChanged);
        guardCountInput.onValueChanged.AddListener(GuardCountChanged);

        artFocusList = Level.Instance.MedievalArtList;
        thiefStartCameraTransform = Level.Instance.ThiefStartList[0];
        if (NavMesh.SamplePosition(thiefStartCameraTransform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            Level.Instance.Thief.NavAgent.Warp(hit.position);

        guardCountMax = Level.Instance.GuardList.Count;
        currentGuardCount = guardCountMax;
        guardCountInput.SetTextWithoutNotify(currentGuardCount.ToString());

        UpdateLevelCameraView();
    }

    /// <summary>
    /// Change the start screen camera's focus based on what parameter is being changed
    /// </summary>
    public void ChangeCameraFocus(OptionFocus newFocus)
    {
        currentFocus = newFocus;
        UpdateLevelCameraView();
    }

    private void LeftButtonPressed()
    {
        // Scroll backwards through the current focused option
        switch (currentFocus)
        {
            case OptionFocus.Steal:
                artFocusIndex = artFocusIndex == 0 ? artFocusList.Count - 1 : artFocusIndex - 1;
            break;

            case OptionFocus.ThiefStart:
                startTargetDropdown.value =
                    startTargetDropdown.value == 0 ? startTargetDropdown.options.Count - 1 : startTargetDropdown.value - 1;
                thiefStartCameraTransform = Level.Instance.ThiefStartList[startTargetDropdown.value];
            break;

            case OptionFocus.Guard:
                currentGuardCount = currentGuardCount == 0 ? guardCountMax : currentGuardCount - 1;
                guardCountInput.text = currentGuardCount.ToString();
            break;
        }
        UpdateLevelCameraView();
    }

    private void RightButtonPressed()
    {
        // Scroll forwards through the current focused option
        switch (currentFocus)
        {
            case OptionFocus.Steal:
                artFocusIndex = (artFocusIndex + 1) % artFocusList.Count;
            break;

            case OptionFocus.ThiefStart:
                startTargetDropdown.value = (startTargetDropdown.value + 1) % startTargetDropdown.options.Count;
                thiefStartCameraTransform = Level.Instance.ThiefStartList[startTargetDropdown.value];
                break;

            case OptionFocus.Guard:
                currentGuardCount = (currentGuardCount + 1) % (guardCountMax+1);
                guardCountInput.text = currentGuardCount.ToString();
            break;
        }
        UpdateLevelCameraView();
    }

    private void ConfirmButton()
    {
        GameController.Instance.StartGame(currentGuardCount, artFocusList[artFocusIndex].transform);
        gameObject.SetActive(false);
        levelViewCamera.enabled = false;
    }

    private void InfoButtonPressed()
    {
        // Toggle info screen
        infoScreen.SetActive(!infoScreen.activeSelf);
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void ArtCategoryChanged(int index)
    {
        // Update which kind of art we're stealing and how far away
        // at minimum the camera should be to look at those
        currentFocus = OptionFocus.Steal;
        switch (artCategoryDropdown.options[index].text.ToLower())
        {
            case "medieval":
                artFocusList = Level.Instance.MedievalArtList;
                artCameraDistance = 2.5f;
                break;

            case "abstract":
                artFocusList = Level.Instance.AbstractArtList;
                artCameraDistance = 2.5f;
                break;

            case "sculpture":
                artFocusList = Level.Instance.SculptureArtList;
                artCameraDistance = 1.5f;
                break;
        }
        artFocusIndex = 0;
        UpdateLevelCameraView();
    }

    private void ThiefStartTargetChanged(int index)
    {
        thiefStartCameraTransform = Level.Instance.ThiefStartList[index];
        currentFocus = OptionFocus.ThiefStart;
        UpdateLevelCameraView();
    }

    private void GuardCountChanged(string text)
    {
        if(int.TryParse(text, out int result))
        {
            if (result < 0 || result > guardCountMax)
            {
                result = Mathf.Clamp(result, 0, guardCountMax);
                guardCountInput.SetTextWithoutNotify(result.ToString());
            }
            currentGuardCount = result;
            currentFocus = OptionFocus.Guard;
            UpdateLevelCameraView();
        }
    }

    private void UpdateLevelCameraView()
    {
        levelViewCamera.orthographic = false;
        switch (currentFocus)
        {
            case OptionFocus.Steal:
                Transform artFocusTransform = artFocusList[artFocusIndex].transform;
                BoxCollider artFocusBox = artFocusList[artFocusIndex];

                // Push our camera far enough away to adequately frame the art.
                // Get the largest world bound size of the collider as the unit length
                // to move away as all our art pieces are rotated and scaled with no consistency
                levelViewCamera.transform.position = artFocusTransform.position +
                    artFocusTransform.forward *
                    Mathf.Clamp(
                        Mathf.Max(
                        artFocusBox.bounds.size.x,
                        artFocusBox.bounds.size.y,
                        artFocusBox.bounds.size.z), artCameraDistance, 5f);

                levelViewCamera.transform.LookAt(artFocusTransform);
                break;

            case OptionFocus.ThiefStart:
                levelViewCamera.transform.position = thiefStartCameraTransform.position;
                levelViewCamera.transform.rotation = thiefStartCameraTransform.rotation;
                if(NavMesh.SamplePosition(thiefStartCameraTransform.position, out NavMeshHit hit, 5f,
                    Level.Instance.Thief.NavAgent.areaMask))
                {
                    Level.Instance.Thief.NavAgent.Warp(hit.position);
                }
                break;

            case OptionFocus.Guard:
                // Place our camera above and look down to give a view of the entire level
                levelViewCamera.transform.position = Level.Instance.LevelMiddlePoint + Vector3.up * 40f;
                levelViewCamera.transform.LookAt(Level.Instance.LevelMiddlePoint);
                levelViewCamera.transform.Rotate(Vector3.forward, 270f);
                levelViewCamera.orthographic = true;

                var guardList = Level.Instance.GuardList;
                for(int i = 0; i < guardCountMax; ++i)
                    guardList[i].gameObject.SetActive(i < currentGuardCount);
                break;
        }
    }
}
