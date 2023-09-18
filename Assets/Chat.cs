using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Chat : MonoBehaviour
{
    Tcp tcp;

    public TMP_InputField chat;
    public TMP_InputField id;

    List<string> list;
    public TMP_Text[] text;
    public Image backUI;

    public GameObject[] player;


    void Start()
    {
        tcp = GetComponent<Tcp>();
        list = new List<string>();
    }

    public void BeginServer()
    {
        //��������
        tcp.StartServer(10000, 10);
        player[0].SetActive(true);

        tcp.name = id.text;
    }

    public void BeginClient()
    {
        //Ŭ���̾�Ʈ ����
        tcp.Connect("127.0.0.1", 10000);
        tcp.name = id.text;
    }

    void Update()
    {
        if (tcp != null && tcp.IsConnect())
        {
            byte[] bytes = new byte[1024];
            int length = tcp.Receive(ref bytes, bytes.Length);
            if (length > 0)
            {
                string str = System.Text.Encoding.UTF8.GetString(bytes);

                // ä�� ����Ÿ �޾��� ��
                AddTalk(str);
            }

            UpdateUI();

            //���� Ű �Է��� �����Ͽ� ä�� ������
           if (Input.GetKeyDown(KeyCode.Return))
            {
                SendTalk();
            }
        }
    }
    public void AddTalk(string str)
    {
        while (list.Count >= 3)
        {
            list.RemoveAt(0);
        }

        list.Add(str);
        UpdateTalk();
    }

    public void SendTalk()
    {
        string str = tcp.name + ": " + chat.text;
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
        tcp.Send(bytes, bytes.Length);

        // ä�� ���� ��
        AddTalk(str);

        chat.text = "";
    }

    void UpdateTalk()
    {
        for (int i = 0; i < list.Count; i++)
        {
            text[i].text = list[i];
        }
    }

    void UpdateUI()
    {
        if (!backUI.IsActive())
        {
            backUI.gameObject.SetActive(true);
        }
    }
}
