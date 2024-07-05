using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    private static Level _instance;

    public static Level Instance
    {
        get{
            if (_instance == null)
                _instance = FindObjectOfType<Level>();

            return _instance;
        }
        set => _instance = value;
    }

    private Level() { }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else if(_instance != this)
            Destroy(gameObject);
    }

    [SerializeField]
    private List<GameObject> artList;

    // Start is called before the first frame update
    void Start()
    {
        GameController.Instance.ArtGoal = artList[0].transform;
    }
}
