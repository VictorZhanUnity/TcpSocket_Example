using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class SocketServer : MonoBehaviour
{
    private Socket serverSocket;
    private Socket clientSocket;
    private IPEndPoint ipEnd;
    private int port;

    private IPEndPoint clientInfo;

    private string reciveMsg;
    private string sendMsg;

    private byte[] reciveData = new byte[1024];
    private byte[] sendData = new byte[1024];

    private int reciveDataLength;

    private Thread connectThread;
    private int maxConnect;

    public UnityAction OnStartServer, OnCloseServer, OnClientConnected;
    public UnityAction<string> OnReciveDataFromClient;

    /// <summary>
    /// 開啟Socket
    /// </summary>
    /// <param name="serverport">Server埠號</param>
    /// <param name="maxConnectClicnet">最大連線數</param>
    public void OpenSocket(int serverport = 8080, int maxConnectClicnet = 10)
    {
        port = serverport;
        maxConnect = maxConnectClicnet;
        ipEnd = new IPEndPoint(IPAddress.Any, port); //偵聽任何IP，port埠號
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(ipEnd); //偵聽連線
        serverSocket.Listen(maxConnect); //最大連線數

        //開啟一個執行緒處理連線資料，避免主執行緒處理資料會卡死
        connectThread = new Thread(new ThreadStart(SocketReciveData));
        connectThread.Start();

        OnStartServer?.Invoke();
    }

    /// <summary>
    /// 當接收到Client傳來資料
    /// </summary>
    private void SocketReciveData()
    {
        GetClientInfo();
        while (true)
        {
            reciveData = new byte[1024];
            reciveDataLength = clientSocket.Receive(reciveData);
            if(reciveDataLength == 0)
            {
                GetClientInfo();
                continue;
            }
            reciveMsg = Encoding.ASCII.GetString(reciveData, 0, reciveDataLength);
            print("From Client Message " + reciveMsg);

            OnReciveDataFromClient?.Invoke(reciveMsg);
        }
    }

    /// <summary>
    /// 將string轉成byte送給Client端
    /// </summary>
    private void SocketSend(string sendMsg)
    {
        //清空傳送資料快取
        sendData = new byte[1024];
        //將String轉換成byte，供clientSocket傳送
        sendData = Encoding.ASCII.GetBytes(sendMsg);
        clientSocket.Send(sendData, sendData.Length, SocketFlags.None);
    }

    /// <summary>
    /// 取得Client連線資訊
    /// </summary>
    private void GetClientInfo()
    {
        clientSocket?.Close();
        print("Waiting  for a client");
        clientSocket = serverSocket.Accept();

        //取得Client的IP和Port
        clientInfo = (IPEndPoint) clientSocket.RemoteEndPoint;
        print("Connect with Client " + clientInfo.Address.ToString() + ":" + clientInfo.Port.ToString());
        //送資料給Client端
        sendMsg = "Welcome to my server";
        SocketSend(sendMsg);
    }

    public void CloseSocket()
    {
        clientSocket?.Close();
        connectThread?.Interrupt();
        connectThread?.Abort();
        serverSocket.Close();
        print("Disconnect");
    }
}

