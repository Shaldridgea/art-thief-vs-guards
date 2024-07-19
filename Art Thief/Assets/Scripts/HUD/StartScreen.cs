using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        UpdateLevelCameraView();
    }

    // Update is called once per frame
    void Update()
    {
        
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
                artFocusIndex = (artFocusIndex - 1) % artFocusList.Count;
            break;

            case CameraFocus.Start:
                startTargetDropdown.value = (startTargetDropdown.value - 1) % startTargetDropdown.options.Count;
                thiefStartFocus = Level.Instance.ThiefStartList[startTargetDropdown.value];
            break;

            case CameraFocus.Guard:
                currentGuardCount = (currentGuardCount - 1) % guardCountMax;
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
        
    }

    private void ArtCategoryChanged(int index)
    {
        currentFocus = CameraFocus.Steal;
        switch (artCategoryDropdown.options[index].text.ToLower())
        {
            case "medieval":
                artFocusList = Level.Instance.MedievalArtList;
                break;

            case "abstract":
                artFocusList = Level.Instance.AbstractArtList;
                break;

            case "sculpture":
                artFocusList = Level.Instance.SculptureArtList;
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
        }
        currentFocus = CameraFocus.Guard;
        UpdateLevelCameraView();
    }

    private void UpdateLevelCameraView()
    {
        levelViewCamera.orthographic = false;
        switch (currentFocus)
        {
            case CameraFocus.Steal:
                Transform artFocusTransform = artFocusList[artFocusIndex].transform;
                BoxCollider artFocusBox = artFocusList[artFocusIndex];
                levelViewCamera.transform.position = artFocusTransform.position + artFocusTransform.forward *
                    Mathf.Clamp(
                        Mathf.Max(
                         artFocusBox.bounds.size.x,
                        artFocusBox.bounds.size.y,
                        artFocusBox.bounds.size.z), 2.5f, 5f);

                levelViewCamera.transform.LookAt(artFocusTransform);
                break;

            case CameraFocus.Start:
                levelViewCamera.transform.position = thiefStartFocus.position;
                levelViewCamera.transform.rotation = thiefStartFocus.rotation;
                break;

            case CameraFocus.Guard:
                levelViewCamera.transform.position = Level.Instance.LevelMiddlePoint + Vector3.up * 40f;
                levelViewCamera.transform.LookAt(Level.Instance.LevelMiddlePoint);
                levelViewCamera.transform.Rotate(Vector3.forward, 270f);
                levelViewCamera.orthographic = true;
                break;
        }
    }
}
