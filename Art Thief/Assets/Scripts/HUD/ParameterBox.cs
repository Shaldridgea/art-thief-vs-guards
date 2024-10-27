using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ParameterBox : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField]
    private StartScreen startScreen;

    [SerializeField]
    private StartScreen.OptionFocus boxFocus;

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        startScreen.ChangeCameraFocus(boxFocus);
    }
}
