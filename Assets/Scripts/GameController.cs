using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum GameState { intro, waiting, animateUp, animateTo, animateDown, gameover}
public class GameController : MonoBehaviour {

    [SerializeField]
    GameObject _Peg1;
    Vector3 m_Peg1_Bot_Location;
    [SerializeField]
    GameObject _Peg2;
    Vector3 m_Peg2_Bot_Location;
    [SerializeField]
    GameObject _Peg3;
    Vector3 m_Peg3_Bot_Location;
    [SerializeField]
    GameObject _Peg4;
    Vector3 m_Peg4_Bot_Location;

    [SerializeField]
    GameObject _Disk1;
    [SerializeField]
    GameObject _Disk2;
    [SerializeField]
    GameObject _Disk3;
    [SerializeField]
    GameObject _Disk4;
    [SerializeField]
    GameObject _Disk5;
    [SerializeField]
    GameObject _Disk6;

    private GameState m_State;
    private float m_DiskSize = 0.4f;
    private float m_TopOfPegs = 6f;
    private float m_BotOfPegs = -1.7f;


    [SerializeField]
    float m_MoveSpeed = 2f;
    private Vector3 m_DestinationTransform;

    private List<List<int>> m_PegContent = new List<List<int>>();
    private List<int> m_P1 = new List<int>();
    private List<int> m_P2 = new List<int>();
    private List<int> m_P3 = new List<int>();
    private List<int> m_P4 = new List<int>();

    private Dictionary<string, GameObject> m_Pegs = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> m_Disks = new Dictionary<string, GameObject>();

    private GameObject m_SelectedDisk;

    public int _NumberOfDisks = 6;
    public int _NumberOfPegs = 4;

    //Camera section
    private int m_CurrentCamPOS = 1;
    [SerializeField]
    List<Transform> m_CameraPositions;
    private Transform m_MainCam;

    [SerializeField]
    LayerMask m_RaycastMask;

    // Use this for initialization
    void Start () {
        m_MainCam = Camera.main.transform;
        m_State = GameState.intro;
        m_SelectedDisk = null;
        
        m_PegContent.Add(m_P1);
        m_PegContent.Add(m_P2);
        m_PegContent.Add(m_P3);
        if (_NumberOfPegs == 4)
        {
            m_PegContent.Add(m_P4);
        }
        for (int i = _NumberOfDisks; i > 0; --i)
        {
            m_PegContent[0].Add(i);
        }

        GameObject[] disks = GameObject.FindGameObjectsWithTag("disk");
        foreach (GameObject d in disks)
        {
            m_Disks.Add(d.name, d);
        }
        GameObject[] pegs = GameObject.FindGameObjectsWithTag("peg");
        foreach (GameObject p in pegs)
        {
            m_Pegs.Add(p.name, p);
        }

        m_State = GameState.waiting;
    }


    //Camera Functions
    public void ChangeView(int dir)
    {
        m_CurrentCamPOS += dir;
        if (m_CurrentCamPOS < 0)
        {
            m_CurrentCamPOS = m_CameraPositions.Count - 1;
        }
        if (m_CurrentCamPOS == m_CameraPositions.Count)
        {
            m_CurrentCamPOS = 0;
        }
        m_CamStartTime = Time.time;
        m_CamStartRot = m_MainCam.rotation;
        m_CamStartPos = m_MainCam.position;
        m_CamEndPos = m_CameraPositions[m_CurrentCamPOS].position;
        m_CamEndRotation = m_CameraPositions[m_CurrentCamPOS].rotation;
        m_IsCameraMoving = true;
    }
    private Quaternion m_CamStartRot;
    private Quaternion m_CamEndRotation;
    private Vector3 m_CamStartPos;
    private Vector3 m_CamEndPos;
    [SerializeField]
    private float m_CamMovementDuration = 3f;
    private float m_CamStartTime;
    private bool m_IsCameraMoving = false;
    void MoveCamera()
    {
        float frac = (Time.time - m_CamStartTime) / m_CamMovementDuration;
        m_MainCam.rotation = Quaternion.Lerp(m_CamStartRot, m_CamEndRotation, frac);
        m_MainCam.position = Vector3.Slerp(m_CamStartPos, m_CamEndPos, frac);
        if (frac >= 1f)
        {
            m_IsCameraMoving = false;
        }
    }

	// Update is called once per frame
	void Update () {

        CheckState();

        ClickCheck();

        if (m_IsCameraMoving)
        {
            MoveCamera();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeView(1);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeView(-1);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Move(1,2,_Disk6);
            m_AnimateStartPOS = _Disk1.transform.position;
            m_AnimateEndPOS = new Vector3(_Peg1.transform.position.x, m_TopOfPegs, _Peg1.transform.position.z);
            m_AnimationStartTime = Time.time;
            m_State = GameState.animateUp;
        }
	}

    void ClickCheck()
    {
        if (Input.GetMouseButtonDown(0) && m_State ==  GameState.waiting)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f, m_RaycastMask))
            {
                Debug.Log(hit.collider.gameObject.name);
                if (hit.collider.gameObject.tag == "disk")
                {
                    m_SelectedDisk = hit.collider.gameObject;
                }
                if (hit.collider.gameObject.tag == "peg" && m_SelectedDisk != null)
                {
                    Debug.Log("Disk + Peg selected");
                    int selectDisk = int.Parse(m_SelectedDisk.name);
                    int fromLocation = -1;
                    for (int i = 0; i < m_PegContent.Count; ++i)
                    {
                        if (m_PegContent[i].IndexOf(selectDisk) != -1)
                        {
                            fromLocation = i+1;
                            break;
                        }
                    }
                    Move(fromLocation ,int.Parse( hit.collider.gameObject.name), m_SelectedDisk);
                }
            }
                
        }
    }

    void CheckState()
    {
        switch (m_State)
        {
            case GameState.intro:
                break;
            case GameState.waiting:
                break;
            case GameState.animateUp:
                AnimateUp();
                break;
            case GameState.animateTo:
                AnimateTo();
                break;
            case GameState.animateDown:
                AnimateDown();
                break;
            case GameState.gameover:
                break;
        }
    }
    //from peg X to peg Y moving Z object.
    public void Move(int from, int to, GameObject toMove)
    {
        if (CheckMoveLegality(from, to, int.Parse( toMove.name)))
        {

            Transform fromT = m_Pegs[from.ToString()].transform;
            m_DestinationTransform = AddDiskToPeg(to, int.Parse(toMove.name));
            m_AnimateStartPOS = toMove.transform.position;
            m_AnimateEndPOS = new Vector3(fromT.position.x, m_TopOfPegs, fromT.position.z);
            m_AnimationStartTime = Time.time;
            m_State = GameState.animateUp;
        }
    }

    Vector3 AddDiskToPeg(int peg, int disk)
    {

        for (int i = 0; i < m_PegContent.Count; ++i)
        {
            if (m_PegContent[i].IndexOf(disk) != -1)
            {
                m_PegContent[i].Remove(disk);
                break;
            }
        }
        m_PegContent[peg - 1].Add(disk);

        int numberOfDisksOnPeg = m_PegContent[peg - 1].Count;
        Vector3 restingPOS = new Vector3(
            m_Pegs[peg.ToString()].transform.position.x,
            m_Pegs[peg.ToString()].transform.position.y + m_BotOfPegs + (m_DiskSize * numberOfDisksOnPeg),
            m_Pegs[peg.ToString()].transform.position.z
            );
        return restingPOS;
    }

    bool CheckMoveLegality(int from, int to, int disk)
    {
        Debug.Log("Checking legality of: " + from + " to " + to + " for disk " + disk);
        if (disk != m_PegContent[from - 1][m_PegContent[from - 1].Count - 1])
        {

            Debug.Log("Disk not on top. Disk is at " + m_PegContent[from - 1].IndexOf(disk) + " and top element is " + m_PegContent[from - 1][m_PegContent[from - 1].Count - 1]);
            return false;
        }
        if (m_PegContent[to - 1].Count == 0)
        {
            Debug.Log("There are no disks on target peg.");
            return true;
        }
        else if (m_PegContent[to - 1][m_PegContent[to - 1].Count - 1] < disk)
        {
            Debug.Log("Cannot move on top of a smaller disk.");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Animations
    /// </summary>
    /// <param name="toMove"></param>
    /// 
    private Vector3 m_AnimateStartPOS = Vector3.zero;
    private Vector3 m_AnimateEndPOS = Vector3.zero;
    [SerializeField]
    private float m_AnimationDuration = 2f;
    private float m_AnimationStartTime = 0f;

    void AnimateUp()
    {
        Debug.Log(m_AnimateStartPOS);
        Debug.Log(m_AnimateEndPOS);
        Debug.Log(m_AnimationStartTime);
        float fracComplete = (Time.time - m_AnimationStartTime) / m_AnimationDuration;
        Debug.Log(fracComplete);
        m_SelectedDisk.transform.position =  Vector3.Lerp(m_AnimateStartPOS, m_AnimateEndPOS, fracComplete);
        if (fracComplete >= 1f)
        {
            m_AnimateStartPOS = m_SelectedDisk.transform.position;
            m_AnimateEndPOS = new Vector3(m_DestinationTransform.x, m_TopOfPegs, m_DestinationTransform.z);
            m_AnimationStartTime = Time.time;
            m_State = GameState.animateTo;
        }
    }
    
    void AnimateTo()
    {
        float fracComplete = (Time.time - m_AnimationStartTime) / m_AnimationDuration;
        m_SelectedDisk.transform.position = Vector3.Lerp(m_AnimateStartPOS, m_AnimateEndPOS, fracComplete);
        if (fracComplete >= 1f)
        {
            m_AnimateStartPOS = m_SelectedDisk.transform.position;
            //int numberOfDisksOnPeg = m_PegContent[int.Parse( m_DestinationTransform.gameObject.name) -1].Count;
            m_AnimateEndPOS = new Vector3(
                m_DestinationTransform.x, 
                m_DestinationTransform.y ,
                m_DestinationTransform.z);
            m_AnimationStartTime = Time.time;
            m_State = GameState.animateDown;
        }
    }
    
    void AnimateDown()
    {
        float fracComplete = (Time.time - m_AnimationStartTime) / m_AnimationDuration;
        m_SelectedDisk.transform.position = Vector3.Lerp(m_AnimateStartPOS, m_AnimateEndPOS, fracComplete);
        if (fracComplete >= 1f)
        {
            m_State = GameState.waiting;
            m_SelectedDisk = null;
        }
    }
}
