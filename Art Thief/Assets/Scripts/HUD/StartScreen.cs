using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AI;

public class StartScreen : MonoBehaviour
{
    public enum CameraFocus
    {
        Steal,
        Start,
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
    private TMP_Dropdown artCategoryDropdown;

    [SerializeField]
    private TMP_Dropdown startTargetDropdown;

    [SerializeField]
    private TMP_InputField guardCountInput;

    [SerializeField]
    private Camera levelViewCamera;

    private CameraFocus currentFocus;

    private int currentGuardCount;

    private int guardCountMax = 6;

    private List<BoxCollider> artFocusList;

    private int artFocusIndex;

    private float artCameraDistance = 2.5f;

    private Transform thiefStartFocus;

    // Start is called before the first frame update
    void Start()
    {
        leftButton.onClick.AddListener(LeftButtonPressed);
        rightButton.onClick.AddListener(RightButtonPressed);
        confirmButton.onClick.AddListener(ConfirmButton);

        artCategoryDropdown.onValueChanged.AddListener(ArtCategoryChanged);
        startTargetDropdown.onValueChanged.AddListener(StartTargetChanged);
        guardCountInput.onValueChanged.AddListener(GuardCountChanged);

        artFocusList = Level.Instance.MedievalArtList;
        thiefStartFocus = Level.Instance.ThiefStartList[0];
        if (NavMesh.SamplePosition(thiefStartFocus.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            Level.Instance.Thief.NavAgent.Warp(hit.position);

        guardCountMax = Level.Instance.GuardList.Count;
        currentGuardCount = guardCountMax;
        guardCountInput.SetTextWithoutNotify(currentGuardCount.ToString());

        UpdateLevelCameraView();
    }

    public void ChangeCameraFocus(CameraFocus newFocus)
    {
        currentFocus = newFocus;
        UpdateLevelCameraView();
    }

    private void LeftButtonPressed()
    {
        switch (currentFocus)
        {
            case CameraFocus.Steal:
                artFocusIndex = artFocusIndex == 0 ? artFocusList.Count - 1 : artFocusIndex - 1;
            break;

            case CameraFocus.Start:
                startTargetDropdown.value =
                    startTargetDropdown.value == 0 ? startTargetDropdown.options.Count - 1 : startTargetDropdown.value - 1;
                thiefStartFocus = Level.Instance.ThiefStartList[startTargetDropdown.value];
            break;

            case CameraFocus.Guard:
                currentGuardCount = currentGuardCount == 0 ? guardCountMax - 1 : currentGuardCount - 1;
                guardCountInput.text = currentGuardCount.ToString();
            break;
        }
        UpdateLevelCameraView();
    }

    private void RightButtonPressed()
    {
        switch (currentFocus)
        {
            case CameraFocus.Steal:
                artFocusIndex = (artFocusIndex + 1) % artFocusList.Count;
            break;

            case CameraFocus.Start:
                startTargetDropdown.value = (startTargetDropdown.value + 1) % startTargetDropdown.options.Count;
                thiefStartFocus = Level.Instance.ThiefStartList[startTargetDropdown.value];
                break;

            case CameraFocus.Guard:
                currentGuardCount = (currentGuardCount + 1) % guardCountMax;
                guardCountInput.text = currentGuardCount.ToString();
            break;
        }
        UpdateLevelCameraView();
    }

    private void ConfirmButton()
    {
        GameController.Instance.StartGame(currentGuardCount, artFocusList[artFocusIndex].transform);
        gameObject.SetActive(false);
    }

    private void ArtCategoryChanged(int index)
    {
        currentFocus = CameraFocus.Steal;
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

    private void StartTargetChanged(int index)
    {
        thiefStartFocus = Level.Instance.ThiefStartList[index];
        currentFocus = CameraFocus.Start;
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
            currentFocus = CameraFocus.Guard;
            UpdateLevelCameraView();
        }
    }

    private void UpdateLevelCameraView()
    {
        levelViewCamera.orthographic = false;
        switch (currentFocus)
        {
            case CameraFocus.Steal:
                Transform artFocusTransform = artFocusList[artFocusIndex].transform;
                BoxCollider artFocusBox = artFocusList[artFocusIndex];

                // Push our camera far enough away to adequately frame the painting
                // Get the largest world bound size of the collider as the unit length
                // to move away as all our paintings are rotated and scaled with no consistency
                levelViewCamera.transform.position = artFocusTransform.position +
                    artFocusTransform.forward *
                    Mathf.Clamp(
                        Mathf.Max(
                         artFocusBox.bounds.size.x,
                        artFocusBox.bounds.size.y,
                        artFocusBox.bounds.size.z), artCameraDistance, 5f);

                levelViewCamera.transform.LookAt(artFocusTransform);
                break;

            case CameraFocus.Start:
                levelViewCamera.transform.position = thiefStartFocus.position;
                levelViewCamera.transform.rotation = thiefStartFocus.rotation;
                if(NavMesh.SamplePosition(thiefStartFocus.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                {
                    Level.Instance.Thief.NavAgent.Warp(hit.position);
                }
                break;

            case CameraFocus.Guard:
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
