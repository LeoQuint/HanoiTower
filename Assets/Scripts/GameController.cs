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

    [SerializeField]
    GameObject _GameOverImage;

    private GameState m_State;
    private float m_DiskSize = 0.4f;
    private float m_TopOfPegs = 6f;
    private float m_BotOfPegs = -1.7f;


    [SerializeField]
    float m_MoveSpeed = 2f;
    private Vector3 m_DestinationTransform;

    private List<Stack<int>> m_PegContent = new List<Stack<int>>();

    private List<Vector3> _StartPos = new List<Vector3>();
   
    private Stack<int> m_P1 = new Stack<int>();
    private Stack<int> m_P2 = new Stack<int>();
    private Stack<int> m_P3 = new Stack<int>();
    private Stack<int> m_P4 = new Stack<int>();

    private Dictionary<string, GameObject> m_Pegs = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> m_Disks = new Dictionary<string, GameObject>();
    //Solver
    private List<Move> m_Solution = new List<Move>();
    private Stack<Move> m_BadMoves = new Stack<Move>();
    private int m_CurrentMove = 0;

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
        for (int i = 0; i < 6; i++)
        {
            _StartPos.Add(Vector3.zero);
        }
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
            m_PegContent[0].Push(i);
        }

        GameObject[] disks = GameObject.FindGameObjectsWithTag("disk");
        foreach (GameObject d in disks)
        {
            _StartPos[int.Parse(d.name)-1] = (d.transform.position);
            m_Disks.Add(d.name, d);
        }
        GameObject[] pegs = GameObject.FindGameObjectsWithTag("peg");
        foreach (GameObject p in pegs)
        {
            m_Pegs.Add(p.name, p);
        }
        CalculateSolution();
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

        KeyboardInput();

        if (m_IsCameraMoving)
        {
            MoveCamera();
        }       
	}

    void KeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeView(1);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeView(-1);
        }
        if (Input.GetKeyDown(KeyCode.Space) && m_State == GameState.waiting)
        {
            Move m = GetNextMove();
            Debug.Log("CPU suggest: " + m.from + " to " + m.to);
            m_SelectedDisk = m_Disks[m_PegContent[m.from - 1].Peek().ToString()];
            Move(m.from, m.to, m_SelectedDisk);
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
                        if (m_PegContent[i].Contains(selectDisk))
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

    private float gameOverPopTimer = 0f;

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
                if (!_GameOverImage.activeSelf)
                {
                    _GameOverImage.SetActive(true);
                }
                if(_GameOverImage.transform.localScale.x < 1f)
                _GameOverImage.transform.localScale = Vector3.one * (Time.time - gameOverPopTimer);
                break;
        }
    }
    //from peg X to peg Y moving Z object.
    public void Move(int from, int to, GameObject toMove)
    {
        
        if (CheckMoveLegality(from, to, int.Parse( toMove.name)))
        {
            Move latest = new Move(from, to);
            if (m_Solution[m_CurrentMove] == latest)
            {
                m_CurrentMove++;
            }
            else if (m_BadMoves.Count > 0 && latest == m_BadMoves.Peek().Reverse())
            {
                m_BadMoves.Pop();
            }
            else
            {
                m_BadMoves.Push(latest);
            }
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
            if (m_PegContent[i].Contains(disk))
            {
                int removed = m_PegContent[i].Pop();
                Debug.Log("Popped " + removed);
                break;
            }
        }
        m_PegContent[peg - 1].Push(disk);

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
        if (disk != m_PegContent[from-1].Peek() )
        {
            Debug.Log("Disk not on top.");
            return false;
        }
        if (m_PegContent[to - 1].Count == 0)
        {
            Debug.Log("There are no disks on target peg.");
            return true;
        }
        else if (m_PegContent[to - 1].Peek() < disk)
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
            if (IsGameOver())
            {
                Debug.Log("GameOver");
                gameOverPopTimer = Time.time;
                m_State = GameState.gameover;
            }
        }
    }

    Move GetNextMove()
    {
        if (m_BadMoves.Count > 0)
        {
            return m_BadMoves.Peek().Reverse();
        }
        else
        {
            return m_Solution[m_CurrentMove];
        }
    }

    bool IsGameOver()
    {
        return m_PegContent[_NumberOfPegs - 1].Count == _NumberOfDisks;
    }
    
    void Solver(int diskCount, int fromPole, int toPole, int viaPole)
    {
        if (diskCount == 1)
        {
            m_Solution.Add(new Move(fromPole, toPole));
            Debug.Log("Move disk from pole " + fromPole + " to pole " + toPole);
        }
        else
        {            
            Solver(diskCount - 1, fromPole, viaPole, toPole);
            Solver(1, fromPole, toPole, viaPole);
            Solver(diskCount - 1, viaPole, toPole, fromPole);
        }
    }

    void CalculateSolution()
    {
        m_Solution.Clear();
        Solver(_NumberOfDisks, 1, _NumberOfPegs, 2);
    }

    public void NewGame(int diff)
    {
        switch (diff)
        {
            case 1:
               
                _NumberOfPegs = 3;
                _NumberOfDisks = 3;
               
                break;
            case 2:
                _NumberOfPegs = 4;
                _NumberOfDisks = 4;
                break;
            case 3:
                _NumberOfPegs = 4;
                _NumberOfDisks = 6;
                break;
        }
        m_State = GameState.intro;
        m_SelectedDisk = null;
        m_P1.Clear();
        m_P2.Clear();
        m_P3.Clear();
        m_P4.Clear();
        m_PegContent.Add(m_P1);
        m_PegContent.Add(m_P2);
        m_PegContent.Add(m_P3);
        if (_NumberOfPegs == 4)
        {
            m_PegContent.Add(m_P4);
        }
        for (int i = _NumberOfDisks; i > 0; --i)
        {
            m_PegContent[0].Push(i);
        }

        for (int i = 1; i <= 6; i++)
        {
            m_Disks[i.ToString()].transform.position = _StartPos[i - 1];
            if (i > _NumberOfDisks)
            {
                m_Disks[i.ToString()].SetActive(false);
            }
            else
            {
                m_Disks[i.ToString()].SetActive(true);
            }
        }
 
        if (_NumberOfPegs == 3)
        {
            m_Pegs["4"].SetActive(false);
        }
        else
        {
            m_Pegs["4"].SetActive(true);
        }
       

        CalculateSolution();
        m_State = GameState.waiting;
        _GameOverImage.SetActive(false);
    }
}

public struct Move
{
    public Move(int f, int t) {
        to = t;
        from = f;
    }
    public int from;
    public int to;

    public static bool operator ==(Move obj1, Move obj2)
    {
        return (obj1.from == obj2.from
                    && obj1.to == obj2.to);
    }
    
    public static bool operator !=(Move obj1, Move obj2)
    {
        return !(obj1.from == obj2.from
                    && obj1.to == obj2.to);
    }

    public Move Reverse()
    {
        return new Move(to, from);
    }

    public override string ToString()
    {
        return " from:"+ from + " to: " + to;
    }
}
