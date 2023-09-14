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

    State state; //게임의 현재 상태
    Stone stoneTurn; //현재 턴
    Stone stoneI; //플레이어 I의 돌
    Stone stoneYou; //플레이어 You의 돌
    Stone stoneWinner; //승자의 돌

    void Start()
    {
        tcp = GetComponent<Tcp>();

        state = State.Start; //게임의 상태를 Start로 설정

        for(int i = 0; i<board.Length; ++i)
        {
            board[i] = (int)Stone.None; //배열의 모든 요소를 Stone.None 정수 값으로 초기화
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
        if (!tcp.IsConnect()) return; //defensive programming, 연결 안됐으면 나가라

        if (state == State.Start)
        {
            UpdateStart(); //게임의 시작
        }

        if (state == State.Game)
        {
            UpdateGame(); //진행 중
        }

        if (state == State.End)
        {
            UpdateEnd(); //종료 상태
        }
    }


    void SetAnimation(bool bWin)
    {
        int iPlayer;

        if (bWin)
        {
            iPlayer = tcp.IsServer() ? 0 : 1;

            //애니메이션 갱신
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

            //애니메이션 갱신
            player[iPlayer].GetComponent<Animator>().SetTrigger("Lose");
        }
        else
            iPlayer = tcp.IsServer() ? 1 : 0;

        player[iPlayer].GetComponent<Animator>().SetTrigger("Lose");

    }

    // 서버에서 클라이언트로 승리 또는 패배 정보를 전송
    void ServerUpdate()
    {
        byte[] data = new byte[1];
        int iSize = tcp.Receive(ref data, data.Length);



        if (iSize > 0)
        {
            if (data[0] == 1) // 클라이언트가 승리 상태를 전달한 경우
            {
                // 승리 상태를 클라이언트에 반영하여 클라이언트의 애니메이션을 설정
                win = true;
                lose = false;
                SetAnimation(win);
                GetAnimation(lose);
            }
            else if (data[0] == 2) // 클라이언트가 패배 상태를 전달한 경우
            {
                // 패배 상태를 클라이언트에 반영하여 클라이언트의 애니메이션을 설정
                win = false;
                lose = true;
                SetAnimation(win);
                GetAnimation(lose);
            }
        }
    }

    void UpdateStart() //게임 시작 초기화
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

    void UpdateGame() //게임 진행 중 로직 처리
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
            Debug.Log("승리: " + (int)stoneWinner);
        }

        stoneTurn = (stoneTurn == Stone.White) ? Stone.Black : Stone.White;
    }

    void UpdateEnd() //애니메이션 구현 및 Restart 버튼 활성화
    {
        if(stoneWinner == stoneI)
        {
            win = true;
            lose = false;

            //승리 상태를 서버에 전송
            byte[] data = new byte[1];
            data[0] = (byte)1; //1을 서버에 전달하여 승리 상태임을 표시
            tcp.Send(data, data.Length);
        }

        else if(stoneWinner == stoneYou)
        {
            win = false;
            lose = true;

            //패배 상태를 서버에 전송
            byte[] data = new byte[1];
            data[0] = (byte)2; //2를 서버에 전달하여 패배 상태임을 표시
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

    bool SetStone(int i, Stone stone) //보드의 특정 위치에 돌을 놓는 메서드, 이미 돌이 있는 경우에 놓을 수 없음
    {
        if (board[i] == (int)Stone.None)
        {
            board[i] = (int)stone;
            return true;
        }
        return false;
    }

    int PosToNumber(Vector3 pos) //마우스 클릭 위치를 기반으로 보드의 인덱스 번호를 반환하는 메서드, -50은 반지름값 고려한 것임
    {
        float boardSize = 500; //바둑판의 크기
        float x = pos.x;
        float y = Screen.height - pos.y; //내가 원하는 마우스 위치값과 실제 위치값이 달라서 -pos.y를 해줘야 함

        if (x < (Screen.width - boardSize) / 2 || x >= (Screen.width - boardSize) / 2 + boardSize)
        {
            return -1;
        }

        if (y < (Screen.height - boardSize) / 2 || y >= (Screen.height - boardSize) / 2 + boardSize)
        {
            return -1;
        }

        int boardRows = 19; //19x19 보드의 행 수
        int boardCols = 19; //19x19 보드의 열 수

        float cellSize = boardSize / boardRows;

        int h = (int)((x - (Screen.width - boardSize) / 2) / cellSize);
        int v = (int)((y - (Screen.height - boardSize) / 2) / cellSize);

        int i = v * boardCols + h;

        return i;
    }

    bool MyTurn() //내 턴 동안 수행되는 로직 처리
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

        Debug.Log("보냄:" + i);

        return true;
    }

    bool YourTurn() //상대 턴 동안 수행되는 로직 처리
    {
        byte[] data = new byte[1];
        int iSize = tcp.Receive(ref data, data.Length);

        if(iSize <= 0)
        {
            return false;
        }

        int i = (int)data[0];
        Debug.Log("받음: " + i);

        bool ret = SetStone(i, stoneYou);
        if(ret == false)
        {
            return false;
        }
        return true;
    }

    Stone CheckBoard() //현재 게임 보드 상에서 승자를 확인하는 메서드
    {
        int size = 19; // 바둑판 크기

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

                    // 가로방향 처리
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

                    // 세로방향 처리
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

                    // 대각선 방향 처리 (좌상에서 우하로)
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

                    // 대각선 방향 처리 (좌하에서 우상으로)
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


    void OnGUI() //GUI를 렌더링할 때 호출되는 메서드
    {
        if (!Event.current.type.Equals(EventType.Repaint))
            return;

        if (state == State.Game)
        {
            //화면 중앙에 위치하도록 Rect의 위치와 크기를 조정합니다.
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            float boardSize = 500; //바둑판의 크기
            float cellSize = boardSize / 19; //보드의 크기를 고려하여 바둑알의 크기와 위치를 조정
            float boardX = (screenWidth - boardSize) / 2;
            float boardY = (screenHeight - boardSize) / 2;

            Graphics.DrawTexture(new Rect(boardX, boardY, boardSize, boardSize), texBoard); //Repaint 이벤트가 발생할 때, 게임 보드 텍스처를 그림)

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

            float turny = boardY - 30; // 턴 표시 위치 조정

            Graphics.DrawTexture(new Rect(turnx, turny, cellSize, cellSize), (stoneTurn == Stone.White) ? texWhite : texBlack);
        }


        if (state == State.End)
        {
            //승자 돌을 표시할 위치를 화면 중앙에 맞춥니다.
            float centerx = (Screen.width - 100) / 2;
            float centery = (Screen.height - 100) / 2;

            Texture tex = (stoneWinner == Stone.White) ? texWhite : texBlack;
            Graphics.DrawTexture(new Rect(centerx, centery+100, 100, 100), tex);

        }

    }
}
