using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using Debug = Managers.DebugHandler.DebugManager;

namespace Managers.TcpSocketHandler
{
    public class TcpSocketServer
    {
        #region {========== Singleton: Instance ==========}
        private static TcpSocketServer _instance;
        public static TcpSocketServer Instance
        {
            get
            {
                if (_instance == null) _instance = new TcpSocketServer();
                return _instance;
            }
        }
        #endregion

        private Socket serverSocket;
        private Socket clientSocket;
        private IPEndPoint serverInfo, clientInfo;
        /// <summary>
        /// Server的IP
        /// </summary>
        public string ServerIP
        {
            get
            {
                IPHostEntry host;
                string localIP = "0.0.0.0";
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
                return localIP;
            }
        }
        public string ServerPort { get { return serverInfo.Port.ToString(); } }

        private string reciveMsg;
        private string sendMsg;

        private byte[] reciveData = new byte[1024];
        private byte[] sendData = new byte[1024];

        private int reciveDataLength;

        private Thread connectThread;

        public bool IsServerOpen
        {
            get
            {
                return serverInfo != null;
            }
        }
        public bool IsHaveClient
        {
            get
            {
                return clientSocket != null;
            }
        }

        /// <summary>
        /// 當開啟Server時觸發
        /// </summary>
        public UnityAction OnStartServer;
        /// <summary>
        /// 當關閉Server時觸發
        /// </summary>
        public UnityAction OnCloseServer;
        /// <summary>
        /// 當偵測到Client連線過來時觸發
        /// </summary>
        public UnityAction<IPEndPoint> OnClientConnected;

        public UnityAction<string> OnReciveDataFromClient;

        /// <summary>
        /// 開啟SocketServer
        /// </summary>
        /// <param name="maxClient">最大連線數</param>
        public void OpenSocket(int port = 8080, int maxClient = 10)
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverInfo = new IPEndPoint(IPAddress.Any, port); //偵聽任何IP，port埠號
            serverSocket.Bind(serverInfo); //偵聽連線
            serverSocket.Listen(maxClient); //最大連線數

            //開啟一個執行緒處理連線資料，避免主執行緒處理資料會卡死
            connectThread = new Thread(new ThreadStart(OnReciveData));
            connectThread.Start();

            OnStartServer?.Invoke();
        }

        /// <summary>
        /// 當接收到Client傳來資料
        /// </summary>
        private void OnReciveData()
        {
            GetClientInfo();
            while (true)
            {
                reciveData = new byte[1024];
                reciveDataLength = clientSocket.Receive(reciveData);
                if (reciveDataLength == 0)
                {
                    GetClientInfo();
                    continue;
                }
                reciveMsg = Encoding.ASCII.GetString(reciveData, 0, reciveDataLength);
                //print("From Client Message " + reciveMsg);

                OnReciveDataFromClient?.Invoke(reciveMsg);
            }
        }

        /// <summary>
        /// 將string轉成byte送給Client端
        /// </summary>
        public void SendDataToClient(string sendMsg)
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
            //print("Waiting  for a client");
            clientSocket = serverSocket.Accept();
            //取得Client的IP和Port
            clientInfo = (IPEndPoint)clientSocket.RemoteEndPoint;
            //print("Connect with Client " + clientInfo.Address.ToString() + ":" + clientInfo.Port.ToString());
            OnClientConnected?.Invoke(clientInfo);
        }

        public void CloseSocket()
        {
            clientSocket?.Close();
            clientSocket = null;
            connectThread?.Interrupt();
            connectThread?.Abort();
            serverSocket.Close();
            serverInfo = null;
            OnCloseServer?.Invoke();
            //print("Disconnect");
        }

        
    }
}
