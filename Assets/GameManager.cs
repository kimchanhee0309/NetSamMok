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

    public float LimitTime = 600.0f; //10분(10*60초)
    private float currentTime;
    public TextMeshProUGUI timeText;


    public bool isGameover { get; private set; }

    private void Awake()
    {
        if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentTime = LimitTime;
        UpdateTimeText();
    }


    void Update()
    {
        if (!isGameover)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0.0f)
            {
                currentTime = 0.0f;
                isGameover = true;
                //게임 종료 로직 추가
            }

            UpdateTimeText();
        }
    }

    private void UpdateTimeText()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timeText.text = string.Format("Time: {0:D2}:{1:D2}", minutes, seconds);
    }
}
