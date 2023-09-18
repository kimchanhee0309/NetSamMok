using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class GameManager : MonoBehaviour
{
    //�̱��� ���ٿ� ������Ƽ
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

    public float turnTime = 10.0f; //�� �ð�(10��)
    private bool isPlayerTurn = true; //�÷��̾� �� ����
   // private float timer; //Ÿ�̸�
    public TextMeshProUGUI timerText;

    public void Start()
    {
        //���� ���� �� Ÿ�̸� ����
        StartTurn();
    }

    public void Update()
    {
        //���� �÷��̾��� ���� �ƴ� ��쿡�� Ÿ�̸Ӹ� ����
        if (!isPlayerTurn)
        {
            turnTime -= Time.deltaTime;
            UpdateTimerText(); //Ÿ�̸� �ؽ�Ʈ ������Ʈ
            if (turnTime <= 0)
            {
                //���� ����ǰ� ��� �÷��̾��� ������ ��ȯ
                EndTurn();
            }
        }
    }

    public void StartTurn()
    {
        //�� ���� �� Ÿ�̸� �ʱ�ȭ
        isPlayerTurn = true;
        UpdateTimerText(); //Ÿ�̸� �ؽ�Ʈ ������Ʈ
    }

    public void EndTurn()
    {
        //�� ���� �� ��� �÷��̾��� ������ ��ȯ
        isPlayerTurn = false;
        //���⿡ ��� �÷��̾��� ������ �߰��� �� ����

        //�� ���� �� �ٽ� �÷��̾��� ������ ��ȯ
        StartTurn();
    }

    //�ٵϵ��� �� �� ȣ���ϴ� �Լ�
    public void PlaceStone()
    {
        //�ٵϵ��� �θ� ���� �ٽ� ����
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
