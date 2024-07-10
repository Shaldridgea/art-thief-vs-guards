using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BTRuntimeNodeUI : MonoBehaviour
{
    [SerializeField]
    private Image nodeImage;

    public Image NodeImage => nodeImage;

    [SerializeField]
    private Outline nodeOutline;

    public Outline NodeOutline => nodeOutline;
}
