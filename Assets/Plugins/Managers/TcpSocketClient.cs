using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.Events;

namespace Managers.TcpSocketHandler
{
    /// <summary>
    /// TCP Socket Client相關處理
    /// 外部可收"OnReciveDataFromClient"事件通知，但僅可用Update方式隨時訪問ReciveMessageList的內容來連動更改UI組件
    /// </summary>
    public class TcpSocketClient
    {
        #region {========== Singleton: Instance ==========}
        private static TcpSocketClient _instance;
        public static TcpSocketClient Instance
        {
            get
            {
                if (_instance == null) _instance = new TcpSocketClient();
                return _instance;
            }
        }
        #endregion

        /// <summary>
        /// Local端IP
        /// </summary>
        public string LocalIP
        {
            get
            {
                return TcpSocketServer.Instance.LocalIP;
            }
        }
        /// <summary>
        /// Server的IP
        /// </summary>
        public string ServerIP 
        { 
            get 
            {
                if (serverInfo == null) return "";
                else return serverInfo.Address.ToString(); 
            } 
        }
        /// <summary>
        /// Server的Port
        /// </summary>
        public string ServerPort 
        { 
            get 
            { 
                if (serverInfo == null) return "";
                else return serverInfo.Port.ToString(); 
            } 
        }
        /// <summary>
        /// 收到Server傳來的訊息，若包含":"符號即存入Dictionary中供外部使用
        /// </summary>
        public Dictionary<string, string> ReciveMessageList
        {
            get { return reciveMsgList; }
        }
        private Dictionary<string, string> reciveMsgList = new Dictionary<string, string>();

        /// <summary>
        /// 是否已連線至TCP Socket Server
        /// </summary>
        public bool IsServerConnnected
        {
            get
            {
                return serverInfo != null;
            }
        }

        #region {========== Observer事件 ==========}
        /// <summary>
        /// 當連線至Server時觸發，送出IP和Port
        /// </summary>
        public UnityAction<string, string> OnServerConnected;
        /// <summary>
        /// 當關閉與Server的連線觸發
        /// </summary>
        public UnityAction OnServerDisconnected;
        /// <summary>
        /// 當收到Server傳資料過來時觸發
        /// </summary>
        public UnityAction<string> OnReciveDataFromServer;
        #endregion

        #region {========== Private變數 ==========}
        private Socket serverSocket;
        private IPEndPoint serverInfo;

        private byte[] reciveData = new byte[1024];
        private byte[] sendData = new byte[1024];
        private int reciveDataLength;

        private Thread connectThread;
        #endregion

        /// <summary>
        /// 連線至TCP Socket Server
        /// </summary>
        public void ConnectToServer(string serverIP, int serverPort = 8080)
        {
            //定義伺服器的IP和埠，埠與伺服器對應
            serverInfo = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
            OnServerConnected?.Invoke(ServerIP, ServerPort);

            //開啟一個執行緒處理連線資料，避免主執行緒處理serverSocket.Accept()會卡死
            connectThread = new Thread(new ThreadStart(OnReciveData));
            connectThread.Start();
        }
        void SocketConnet()
        {
            serverSocket?.Close();
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Connect(serverInfo);
        }

        /// <summary>
        /// 當接收到Client傳來資料
        /// </summary>
        private void OnReciveData()
        {
            SocketConnet();
            while (true)
            {
                reciveData = new byte[1024];
                reciveDataLength = serverSocket.Receive(reciveData); //will stop to listen
                if (reciveDataLength == 0)
                {
                    SocketConnet();
                    continue;
                }
                string reciveMsg = Encoding.ASCII.GetString(reciveData, 0, reciveDataLength);
                OnReciveDataFromServer?.Invoke(reciveMsg);

                if (reciveMsg.Contains(","))
                {
                    string[] list = reciveMsg.Split(',');
                    foreach (string item in list)
                    {
                        ReciveParametersHandler(item);
                    }
                }
                else
                {
                    ReciveParametersHandler(reciveMsg);
                }
            }
        }
        /// <summary>
        /// 當傳來的字串包含":"時，當成參數設定存在Dictionary裡供外部讀取
        /// </summary>
        private void ReciveParametersHandler(string reciveMsg)
        {
            if (reciveMsg.Contains(":"))
            {
                string[] str = reciveMsg.Split(':');
                string key = str[0].Trim(), value = str[1].Trim();
                if (reciveMsgList.ContainsKey(key)) reciveMsgList[key] = value;
                else reciveMsgList.Add(key, value);
            }
        }

        /// <summary>
        /// 將string轉成byte送給Server端
        /// </summary>
        public void SendDataToServer(string sendMsg)
        {
            //清空傳送資料快取
            sendData = new byte[1024];
            //將String轉換成byte，供clientSocket傳送
            sendData = Encoding.ASCII.GetBytes(sendMsg);
            serverSocket.Send(sendData, sendData.Length, SocketFlags.None);
        }

        /// <summary>
        /// 關閉TCP Socket Server
        /// </summary>
        public void DisconnectFromServer()
        {
            SendDataToServer("Client Disconnected");
            serverSocket?.Close();
            serverSocket = null;
            serverInfo = null;
            connectThread?.Interrupt();
            connectThread?.Abort();
            reciveMsgList.Clear();
            OnServerDisconnected?.Invoke();
        }
    }
}