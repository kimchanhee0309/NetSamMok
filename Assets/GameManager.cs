using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class GameManager : MonoBehaviour
{
    //싱글턴 접근용 프로퍼티
    public static GameManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<GameManager>();
            }

            return m_instance;
        }
    }

    private static GameManager m_instance;
    private void Awake()
    {
        if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public float turnTime = 10.0f; //턴 시간(10초)
    private bool isPlayerTurn = true; //플레이어 턴 여부
   // private float timer; //타이머
    public TextMeshProUGUI timerText;

    public void Start()
    {
        //게임 시작 시 타이머 시작
        StartTurn();
    }

    public void Update()
    {
        //현재 플레이어의 턴이 아닌 경우에만 타이머를 감소
        if (!isPlayerTurn)
        {
            turnTime -= Time.deltaTime;
            UpdateTimerText(); //타이머 텍스트 업데이트
            if (turnTime <= 0)
            {
                //턴이 종료되고 상대 플레이어의 턴으로 전환
                EndTurn();
            }
        }
    }

    public void StartTurn()
    {
        //턴 시작 시 타이머 초기화
        isPlayerTurn = true;
        UpdateTimerText(); //타이머 텍스트 업데이트
    }

    public void EndTurn()
    {
        //턴 종료 시 상대 플레이어의 턴으로 전환
        isPlayerTurn = false;
        //여기에 상대 플레이어의 동작을 추가할 수 있음

        //턴 종료 후 다시 플레이어의 턴으로 전환
        StartTurn();
    }

    //바둑돌을 둘 때 호출하는 함수
    public void PlaceStone()
    {
        //바둑돌을 두면 턴을 다시 시작
        StartTurn();
    }

    public void UpdateTimerText()
    {
        if(timerText!=null)
        {
            timerText.text = "Time: " + Mathf.Round(turnTime);
        }
    }
}
