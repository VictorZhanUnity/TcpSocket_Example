using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.Events;

namespace Managers.TcpSocketHandler
{
    /// <summary>
    /// TCP Socket Server相關處理，僅供與Client一對一連線。
    /// 外部可收"OnReciveDataFromClient"事件通知，但僅可用Update方式隨時訪問ReciveMessageList的內容來連動更改UI組件
    /// </summary>
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
        /// <summary>
        /// Server的Port
        /// </summary>
        public string ServerPort { get { return serverInfo.Port.ToString(); } }
        /// <summary>
        /// 收到Client傳來的訊息，若包含":"符號即存入Dictionary中供外部使用
        /// </summary>
        public Dictionary<string, string> ReciveMessageList
        {
            get { return reciveMsgList; }
        }
        private Dictionary<string, string> reciveMsgList = new Dictionary<string, string>();

        /// <summary>
        /// TCP Socket Server是否開著
        /// </summary>
        public bool IsServerOpen
        {
            get
            {
                return serverInfo != null;
            }
        }
        /// <summary>
        /// 是否有Client連線進來
        /// </summary>
        public bool IsHaveClient
        {
            get
            {
                return clientSocket != null;
            }
        }

        #region {========== Observer事件 ==========}
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
        /// <summary>
        /// 當收到Client傳資料過來時觸發
        /// </summary>
        public UnityAction<string> OnReciveDataFromClient;
        #endregion

        #region {========== Private變數 ==========}
        private Socket serverSocket;
        private Socket clientSocket;
        private IPEndPoint serverInfo, clientInfo;

        private byte[] reciveData = new byte[1024];
        private byte[] sendData = new byte[1024];
        private int reciveDataLength;

        private Thread connectThread;
        #endregion

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
            OnStartServer?.Invoke();

            //開啟一個執行緒處理連線資料，避免主執行緒處理serverSocket.Accept()會卡死
            connectThread = new Thread(new ThreadStart(OnReciveData));
            connectThread.IsBackground = true;
            connectThread.Start();
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

        /// <summary>
        /// 當接收到Client傳來資料
        /// </summary>
        private void OnReciveData()
        {
            GetClientInfo();
            while (true)
            {
                reciveData = new byte[1024];
                reciveDataLength = clientSocket.Receive(reciveData); //will stop to listen
                if (reciveDataLength == 0)
                {
                    GetClientInfo();
                    continue;
                }
                string reciveMsg = Encoding.ASCII.GetString(reciveData, 0, reciveDataLength);
                OnReciveDataFromClient?.Invoke(reciveMsg);

                if (reciveMsg.Contains(","))
                {
                    string[] list = reciveMsg.Split(',');
                    foreach(string item in list)
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
        /// 關閉TCP Socket Server
        /// </summary>
        public void CloseSocket()
        {
            SendDataToClient("Server Shutdown");
            clientSocket?.Close();
            clientSocket = null;
            connectThread?.Interrupt();
            connectThread?.Abort();
            serverSocket.Close();
            serverInfo = null;
            reciveMsgList.Clear();
            OnCloseServer?.Invoke();
        }
    }
}