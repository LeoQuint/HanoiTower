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
    [SerializeField]
    float m_MoveSpeed = 2f;

    private List<int> m_Peg1Content = new List<int>();
    private List<int> m_Peg2Content = new List<int>();
    private List<int> m_Peg3Content = new List<int>();
    private List<int> m_Peg4Content = new List<int>();

    // Use this for initialization
    void Start () {
        m_State = GameState.intro;
        m_Peg1_Bot_Location = new Vector3(_Peg1.transform.position.x, _Peg1.transform.position.y - 1.7f, _Peg1.transform.position.z);
        m_Peg2_Bot_Location = new Vector3(_Peg2.transform.position.x, _Peg2.transform.position.y - 1.7f, _Peg2.transform.position.z);
        m_Peg3_Bot_Location = new Vector3(_Peg3.transform.position.x, _Peg3.transform.position.y - 1.7f, _Peg3.transform.position.z);
        m_Peg4_Bot_Location = new Vector3(_Peg4.transform.position.x, _Peg4.transform.position.y - 1.7f, _Peg4.transform.position.z);

        _Disk1.transform.position = m_Peg1_Bot_Location + (Vector3.up * m_DiskSize * 5f);
        _Disk2.transform.position = m_Peg1_Bot_Location + (Vector3.up * m_DiskSize * 4f);
        _Disk3.transform.position = m_Peg1_Bot_Location + (Vector3.up * m_DiskSize * 3f);
        _Disk4.transform.position = m_Peg1_Bot_Location + (Vector3.up * m_DiskSize * 2f);
        _Disk5.transform.position = m_Peg1_Bot_Location + (Vector3.up * m_DiskSize * 1f);
        _Disk6.transform.position = m_Peg1_Bot_Location + (Vector3.up * m_DiskSize * 0f);

        m_Peg1Content.Add(6);
        m_Peg1Content.Add(5);
        m_Peg1Content.Add(4);
        m_Peg1Content.Add(3);
        m_Peg1Content.Add(2);
        m_Peg1Content.Add(1);


    }
	
	// Update is called once per frame
	void Update () {
        CheckState();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Move(1,2,_Disk6);
            m_State = GameState.animateUp;
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
                AnimateUp(_Disk1);
                break;
            case GameState.animateTo:
                AnimateTo(_Disk1);
                break;
            case GameState.animateDown:
                AnimateDown(_Disk1);
                break;
            case GameState.gameover:
                break;
        }
    }

    public void Move(int from, int to, GameObject toMove)
    {

    }

    /// <summary>
    /// Animations
    /// </summary>
    /// <param name="toMove"></param>
    /// 
    private Vector3 m_AnimateStartPOS = Vector3.zero;
    private Vector3 m_AnimateEndStartPOS = Vector3.zero;
    private float m_AnimationDuration = 2f;
    private float m_AnimationStartTime = 0f;

    void AnimateUp(GameObject toMove)
    {
        float fracComplete = (Time.time - m_AnimationStartTime) / m_AnimationDuration;
        toMove.transform.position =  Vector3.Slerp(m_AnimateStartPOS, m_AnimateStartPOS , fracComplete);
        if (fracComplete >= 1f)
        {
            m_AnimateStartPOS = toMove.transform.position;
            m_AnimateEndStartPOS = new Vector3(_Peg2.transform.position.x, m_TopOfPegs, _Peg2.transform.position.z);
            m_AnimationStartTime = Time.time;
            m_State = GameState.animateTo;
        }
    }
    
    void AnimateTo(GameObject toMove)
    {
        float fracComplete = (Time.time - m_AnimationStartTime) / m_AnimationDuration;
        toMove.transform.position = Vector3.Slerp(m_AnimateStartPOS, m_AnimateStartPOS, fracComplete);
        if (fracComplete >= 1f)
        {
            m_AnimateStartPOS = toMove.transform.position;
            m_AnimateEndStartPOS = new Vector3(_Peg2.transform.position.x, m_Peg2_Bot_Location.x, _Peg2.transform.position.z);
            m_AnimationStartTime = Time.time;
            m_State = GameState.animateDown;
        }
    }
    
    void AnimateDown(GameObject toMove)
    {
        float fracComplete = (Time.time - m_AnimationStartTime) / m_AnimationDuration;
        toMove.transform.position = Vector3.Slerp(m_AnimateStartPOS, m_AnimateStartPOS, fracComplete);
        if (fracComplete >= 1f)
        {
            m_State = GameState.waiting;
        }
    }
}
