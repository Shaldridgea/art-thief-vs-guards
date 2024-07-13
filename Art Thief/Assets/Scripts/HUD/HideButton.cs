using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HideButton : MonoBehaviour
{
    [SerializeField]
    private Button hideButton;

    [SerializeField]
    private GameObject hideTarget;

    private void Start()
    {
        hideButton.onClick.AddListener(HideButtonClicked);
    }

    private void HideButtonClicked()
    {
        hideTarget.SetActive(false);
    }
}
