using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    private static Level _instance;

    public static Level Instance {
        get {
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
        else if (_instance != this)
            Destroy(gameObject);
    }

    [SerializeField]
    private List<Transform> levelExits;

    public List<Transform> LevelExits => levelExits;

    [SerializeField]
    private List<GuardAgent> guardList;

    public List<GuardAgent> GuardList => guardList;

    [SerializeField]
    private ThiefAgent thief;

    public ThiefAgent Thief => thief;

    [SerializeField]
    private List<GameObject> artList;

    public List<GameObject> ArtList => artList;

    public List<BoxCollider> MedievalArtList { get; private set; } = new(30);

    public List<BoxCollider> AbstractArtList { get; private set; } = new(15);

    public List<BoxCollider> SculptureArtList { get; private set; } = new(4);

    [SerializeField]
    private List<BoxCollider> medievalArtRooms;

    [SerializeField]
    private List<BoxCollider> abstractArtRooms;

    [SerializeField]
    private List<BoxCollider> sculptureRooms;

    [SerializeField]
    private List<Transform> thiefStartTransforms;

    public List<Transform> ThiefStartList => thiefStartTransforms;

    [SerializeField]
    private Transform levelMiddleTransform;

    public Vector3 LevelMiddlePoint => levelMiddleTransform.position;

    // Start is called before the first frame update
    void Start()
    {
        GetArtByRooms(medievalArtRooms, MedievalArtList);
        GetArtByRooms(abstractArtRooms, AbstractArtList);
        GetArtByRooms(sculptureRooms, SculptureArtList);
    }

    private void GetArtByRooms(List<BoxCollider> roomsList, List<BoxCollider> targetArtList)
    {
        foreach (var c in roomsList)
        {
            Collider[] overlaps = Physics.OverlapBox(
                c.transform.position + c.center, c.size / 2f,
                c.transform.rotation, LayerMask.GetMask("Interest"));

            foreach (var o in overlaps)
            {
                if (o.CompareTag("Art"))
                {
                    targetArtList.Add(o.gameObject.GetComponent<BoxCollider>());
                }
            }
        }
    }
}
