using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

using UnityEngine.UI;
using TMPro;
public class SamMok : MonoBehaviour
{
    enum State
    { 
        Start = 0,
        Game,
        End,
    };

    enum Turn 
    {
        I = 0,
        You,
    };

    enum Stone
    {
        None = 0,
        White,
        Black,
    };

    Tcp tcp;
    public TMP_InputField ip;

    public Texture texBoard;
    public Texture texWhite;
    public Texture texBlack;

    private bool win = false;
    private bool lose = false;
    
    public GameObject[] player;

    int[] board = new int[361]; 

    State state; //������ ���� ����
    Stone stoneTurn; //���� ��
    Stone stoneI; //�÷��̾� I�� ��
    Stone stoneYou; //�÷��̾� You�� ��
    Stone stoneWinner; //������ ��

    void Start()
    {
        tcp = GetComponent<Tcp>();

        state = State.Start; //������ ���¸� Start�� ����

        for(int i = 0; i<board.Length; ++i)
        {
            board[i] = (int)Stone.None; //�迭�� ��� ��Ҹ� Stone.None ���� ������ �ʱ�ȭ
        }
    }

    public void ServerStart()
    {
        tcp.StartServer(10000, 10);
        player[0].SetActive(true);
    }

    public void ClientStart()
    {
        tcp.Connect(ip.text, 10000);
    }

    void Update()
    {
        if (!tcp.IsConnect()) return; //defensive programming, ���� �ȵ����� ������

        if (state == State.Start)
        {
            UpdateStart(); //������ ����
        }

        if (state == State.Game)
        {
            UpdateGame(); //���� ��
        }

        if (state == State.End)
        {
            UpdateEnd(); //���� ����
        }
    }


    void SetAnimation(bool bWin)
    {
        int iPlayer;

        if (bWin)
        {
            iPlayer = tcp.IsServer() ? 0 : 1;

            //�ִϸ��̼� ����
            player[iPlayer].GetComponent<Animator>().SetTrigger("Win");
        }
        else
            iPlayer = tcp.IsServer() ? 1 : 0;

        player[iPlayer].GetComponent<Animator>().SetTrigger("Win");
    }

    void GetAnimation(bool bLose)
    {
        int iPlayer;

        if (bLose)
        {
            iPlayer = tcp.IsServer() ? 0 : 1;

            //�ִϸ��̼� ����
            player[iPlayer].GetComponent<Animator>().SetTrigger("Lose");
        }
        else
            iPlayer = tcp.IsServer() ? 1 : 0;

        player[iPlayer].GetComponent<Animator>().SetTrigger("Lose");

    }

    // �������� Ŭ���̾�Ʈ�� �¸� �Ǵ� �й� ������ ����
    void ServerUpdate()
    {
        byte[] data = new byte[1];
        int iSize = tcp.Receive(ref data, data.Length);



        if (iSize > 0)
        {
            if (data[0] == 1) // Ŭ���̾�Ʈ�� �¸� ���¸� ������ ���
            {
                // �¸� ���¸� Ŭ���̾�Ʈ�� �ݿ��Ͽ� Ŭ���̾�Ʈ�� �ִϸ��̼��� ����
                win = true;
                lose = false;
                SetAnimation(win);
                GetAnimation(lose);
            }
            else if (data[0] == 2) // Ŭ���̾�Ʈ�� �й� ���¸� ������ ���
            {
                // �й� ���¸� Ŭ���̾�Ʈ�� �ݿ��Ͽ� Ŭ���̾�Ʈ�� �ִϸ��̼��� ����
                win = false;
                lose = true;
                SetAnimation(win);
                GetAnimation(lose);
            }
        }
    }

    void UpdateStart() //���� ���� �ʱ�ȭ
    {
        state = State.Game;
        stoneTurn = Stone.White;

        player[0].SetActive(true);
        player[1].SetActive(true);

        if (tcp.IsServer())
        {
            stoneI = Stone.White;
            stoneYou = Stone.Black;
        }
        else
        {
            stoneI = Stone.Black;
            stoneYou = Stone.White;
        }
    }

    void UpdateGame() //���� ���� �� ���� ó��
    {
        bool bSet = false;

        if(stoneTurn == stoneI)
        {
            bSet = MyTurn();
        }
        else
        {
            bSet = YourTurn();
        }

        if (bSet == false)
        {
            return;
        }

        stoneWinner = CheckBoard();

        if(stoneWinner != Stone.None)
        {
            state = State.End;
            UpdateEnd();
            Debug.Log("�¸�: " + (int)stoneWinner);
        }

        stoneTurn = (stoneTurn == Stone.White) ? Stone.Black : Stone.White;
    }

    void UpdateEnd() //�ִϸ��̼� ���� �� Restart ��ư Ȱ��ȭ
    {
        if(stoneWinner == stoneI)
        {
            win = true;
            lose = false;

            //�¸� ���¸� ������ ����
            byte[] data = new byte[1];
            data[0] = (byte)1; //1�� ������ �����Ͽ� �¸� �������� ǥ��
            tcp.Send(data, data.Length);
        }

        else if(stoneWinner == stoneYou)
        {
            win = false;
            lose = true;

            //�й� ���¸� ������ ����
            byte[] data = new byte[1];
            data[0] = (byte)2; //2�� ������ �����Ͽ� �й� �������� ǥ��
            tcp.Send(data, data.Length);
        }
        else
        {
            win = false;
            lose = false;
        }

        SetAnimation(win);
        GetAnimation(lose);
    }

    bool SetStone(int i, Stone stone) //������ Ư�� ��ġ�� ���� ���� �޼���, �̹� ���� �ִ� ��쿡 ���� �� ����
    {
        if (board[i] == (int)Stone.None)
        {
            board[i] = (int)stone;
            return true;
        }
        return false;
    }

    int PosToNumber(Vector3 pos) //���콺 Ŭ�� ��ġ�� ������� ������ �ε��� ��ȣ�� ��ȯ�ϴ� �޼���, -50�� �������� ����� ����
    {
        float boardSize = 500; //�ٵ����� ũ��
        float x = pos.x;
        float y = Screen.height - pos.y; //���� ���ϴ� ���콺 ��ġ���� ���� ��ġ���� �޶� -pos.y�� ����� ��

        if (x < (Screen.width - boardSize) / 2 || x >= (Screen.width - boardSize) / 2 + boardSize)
        {
            return -1;
        }

        if (y < (Screen.height - boardSize) / 2 || y >= (Screen.height - boardSize) / 2 + boardSize)
        {
            return -1;
        }

        int boardRows = 19; //19x19 ������ �� ��
        int boardCols = 19; //19x19 ������ �� ��

        float cellSize = boardSize / boardRows;

        int h = (int)((x - (Screen.width - boardSize) / 2) / cellSize);
        int v = (int)((y - (Screen.height - boardSize) / 2) / cellSize);

        int i = v * boardCols + h;

        return i;
    }

    bool MyTurn() //�� �� ���� ����Ǵ� ���� ó��
    {
        bool bClick = Input.GetMouseButtonDown(0);
        if (!bClick)
        {
            return false;
        }

        Vector3 pos = Input.mousePosition;

        int i = PosToNumber(pos);
        if (i == -1 || board[i] != (int)Stone.None)
        {
            return false;
        }

        bool bSet = SetStone(i, stoneI);
        if(bSet == false)
        {
            return false;
        }

        byte[] data = new byte[1];
        data[0] = (byte)i;
        tcp.Send(data, data.Length);

        Debug.Log("����:" + i);

        return true;
    }

    bool YourTurn() //��� �� ���� ����Ǵ� ���� ó��
    {
        byte[] data = new byte[1];
        int iSize = tcp.Receive(ref data, data.Length);

        if(iSize <= 0)
        {
            return false;
        }

        int i = (int)data[0];
        Debug.Log("����: " + i);

        bool ret = SetStone(i, stoneYou);
        if(ret == false)
        {
            return false;
        }
        return true;
    }

    Stone CheckBoard() //���� ���� ���� �󿡼� ���ڸ� Ȯ���ϴ� �޼���
    {
        int size = 19; // �ٵ��� ũ��

        for (int i = 0; i < 2; i++)
        {
            int s;
            if (i == 0)
                s = (int)Stone.White;
            else
                s = (int)Stone.Black;

            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    int index = row * size + col;

                    // ���ι��� ó��
                    if (col + 4 < size)
                    {
                        bool win = true;
                        for (int j = 0; j < 5; j++)
                        {
                            if (board[index + j] != s)
                            {
                                win = false;
                                break;
                            }
                        }
                        if (win)
                            return (Stone)s;
                    }

                    // ���ι��� ó��
                    if (row + 4 < size)
                    {
                        bool win = true;
                        for (int j = 0; j < 5; j++)
                        {
                            if (board[index + j * size] != s)
                            {
                                win = false;
                                break;
                            }
                        }
                        if (win)
                            return (Stone)s;
                    }

                    // �밢�� ���� ó�� (�»󿡼� ���Ϸ�)
                    if (col + 4 < size && row + 4 < size)
                    {
                        bool win = true;
                        for (int j = 0; j < 5; j++)
                        {
                            if (board[index + j * (size + 1)] != s)
                            {
                                win = false;
                                break;
                            }
                        }
                        if (win)
                            return (Stone)s;
                    }

                    // �밢�� ���� ó�� (���Ͽ��� �������)
                    if (col + 4 < size && row - 4 >= 0)
                    {
                        bool win = true;
                        for (int j = 0; j < 5; j++)
                        {
                            if (board[index + j * (size - 1)] != s)
                            {
                                win = false;
                                break;
                            }
                        }
                        if (win)
                            return (Stone)s;
                    }
                }
            }
        }

        return Stone.None;
    }


    void OnGUI() //GUI�� �������� �� ȣ��Ǵ� �޼���
    {
        if (!Event.current.type.Equals(EventType.Repaint))
            return;

        if (state == State.Game)
        {
            //ȭ�� �߾ӿ� ��ġ�ϵ��� Rect�� ��ġ�� ũ�⸦ �����մϴ�.
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            float boardSize = 500; //�ٵ����� ũ��
            float cellSize = boardSize / 19; //������ ũ�⸦ ����Ͽ� �ٵϾ��� ũ��� ��ġ�� ����
            float boardX = (screenWidth - boardSize) / 2;
            float boardY = (screenHeight - boardSize) / 2;

            Graphics.DrawTexture(new Rect(boardX, boardY, boardSize, boardSize), texBoard); //Repaint �̺�Ʈ�� �߻��� ��, ���� ���� �ؽ�ó�� �׸�)

            for (int i = 0; i < board.Length; ++i)
            {
                if (board[i] != (int)Stone.None)
                {
                    int row = i / 19;
                    int col = i % 19;

                    float x = boardX + col * cellSize;
                    float y = boardY + row * cellSize;

                    Texture tex = (board[i] == (int)Stone.White) ? texWhite : texBlack;
                    Graphics.DrawTexture(new Rect(x, y, cellSize, cellSize), tex);
                }
            }

            float turnx;
            if (stoneTurn == Stone.White)
                turnx = boardX - cellSize / 2;
            else
                turnx = boardX + boardSize - cellSize / 2;

            float turny = boardY - 30; // �� ǥ�� ��ġ ����

            Graphics.DrawTexture(new Rect(turnx, turny, cellSize, cellSize), (stoneTurn == Stone.White) ? texWhite : texBlack);
        }


        if (state == State.End)
        {
            //���� ���� ǥ���� ��ġ�� ȭ�� �߾ӿ� ����ϴ�.
            float centerx = (Screen.width - 100) / 2;
            float centery = (Screen.height - 100) / 2;

            Texture tex = (stoneWinner == Stone.White) ? texWhite : texBlack;
            Graphics.DrawTexture(new Rect(centerx, centery+100, 100, 100), tex);

        }

    }
}
