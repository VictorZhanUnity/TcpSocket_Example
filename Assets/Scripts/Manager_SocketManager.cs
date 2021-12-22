using UnityEngine;
using UnityEngine.UI;
using System.Net;
using Managers.TcpSocketHandler;
using Debug = Managers.DebugHandler.DebugManager;

public class Manager_SocketManager : MonoBehaviour
{
    [SerializeField] private InputField input_ServerPort, input_MsgToClient;
    [SerializeField] private Text text_Console;
    [SerializeField] private Button button_OpenServer, button_CloseServer, button_SendDataToClient, button_ClearConsole;
    [SerializeField] private Toggle toggle_ServerState;

    #region {========= 初始化設定 =========}
    private void Start()
    {
        text_Console.text = "";
        SetFocus_InputField(input_ServerPort);
        InitDebug();
        InitTcpListener();
        InitBtnListener();
    }
    private void InitDebug()
    {
        Debug.IsActivated = true;
        Debug.isRecord = true;
        Debug.onLogEvent += OnLogEvent;
    }
    private void InitTcpListener()
    {
        TcpSocketServer.Instance.OnStartServer += OnStartServer;
        TcpSocketServer.Instance.OnCloseServer += OnCloseServer;
        TcpSocketServer.Instance.OnClientConnected += OnClientConnected;
        TcpSocketServer.Instance.OnReciveDataFromClient += OnReciveDataFromClient;
    }
    private void InitBtnListener()
    {
        button_OpenServer.onClick.AddListener(StartServer);
        button_CloseServer.onClick.AddListener(CloseServer);
        button_SendDataToClient.onClick.AddListener(SendDataToClient);
        button_ClearConsole.onClick.AddListener(ClearConsole);
    }
    public void OnDestroy()
    {
        Debug.onLogEvent -= OnLogEvent;
        TcpSocketServer.Instance.OnStartServer -= OnStartServer;
        TcpSocketServer.Instance.OnCloseServer -= OnCloseServer;
        TcpSocketServer.Instance.OnClientConnected -= OnClientConnected;
        TcpSocketServer.Instance.OnReciveDataFromClient -= OnReciveDataFromClient;
        button_OpenServer.onClick.RemoveListener(OnStartServer);
        button_OpenServer.onClick.RemoveListener(OnStartServer);
        button_CloseServer.onClick.RemoveListener(OnCloseServer);
        button_SendDataToClient.onClick.RemoveListener(SendDataToClient);
    }
    private void Update()
    {
        text_Console.text = Debug.LogHistory;
        if (TcpSocketServer.Instance.ReciveMessageList.Count > 0)
        {
            text_Console.text += "\n Parameters:";
            foreach (string key in TcpSocketServer.Instance.ReciveMessageList.Keys)
            {
                text_Console.text += $"\n{key}:{TcpSocketServer.Instance.ReciveMessageList[key]}";
            }
        }
        UpdateUI();
    }
    private void UpdateUI()
    {
        button_SendDataToClient.interactable = (input_MsgToClient.text.Length > 0);
        button_OpenServer.interactable = (input_ServerPort.text.Length > 0 && !TcpSocketServer.Instance.IsServerOpen);
        button_CloseServer.interactable = TcpSocketServer.Instance.IsServerOpen;
        toggle_ServerState.isOn = TcpSocketServer.Instance.IsServerOpen;
        button_ClearConsole.interactable = (text_Console.text.Length > 0);
        input_MsgToClient.interactable = TcpSocketServer.Instance.IsHaveClient;
        button_SendDataToClient.interactable = ((input_MsgToClient.text.Length > 0) && TcpSocketServer.Instance.IsHaveClient);
        if (TcpSocketServer.Instance.IsHaveClient && !input_MsgToClient.isFocused) SetFocus_InputField(input_MsgToClient);
    }
    #endregion

    private void OnLogEvent(string msg)
    {
        //text_Console.text = Debug.LogHistory;
        // ↑ 無法藉由Thread來更新UI組件的內容，只有Main Thread能夠更新
    }
    public void StartServer()
    {
        TcpSocketServer.Instance.OpenSocket(int.Parse(input_ServerPort.text));
    }
    public void CloseServer()
    {
        TcpSocketServer.Instance.CloseSocket();
    }
    public void SendDataToClient()
    {
        TcpSocketServer.Instance.SendDataToClient(input_MsgToClient.text);
        Debug.Log($"OnSendDataToClient: {input_MsgToClient.text}");
        SetFocus_InputField(input_MsgToClient);

    }
    private void SetFocus_InputField(InputField target)
    {
        target.text = "";
        target.Select();
        target.ActivateInputField();
    }
    public void ClearConsole()
    {
        Debug.ClearHistory();
    }

    #region {========== Observer事件 ==========}
    private void OnStartServer()
    {
        Debug.Log($"===== Server Open: {TcpSocketServer.Instance.ServerIP}: {TcpSocketServer.Instance.ServerPort} =====", Debug.TextColor.white);
    }
    private void OnCloseServer()
    {
        Debug.Log("===== Server Close =====", Debug.TextColor.white);
        Debug.LogSeparater();
        SetFocus_InputField(input_ServerPort);
    }
    private void OnClientConnected(IPEndPoint clientInfo)
    {
        Debug.Log($"Client Connected: {clientInfo.Address}:{clientInfo.Port}", Debug.TextColor.red);
    }
    private void OnReciveDataFromClient(string msg)
    {
        Debug.Log($"Recived Data From Client: {msg}", Debug.TextColor.lime);
    } 
    #endregion

    [ContextMenu("- FindParameters")]
    private void FindParameters()
    {
        input_ServerPort = GameObject.Find("InputField_ServerPort").GetComponent<InputField>();
        input_MsgToClient = GameObject.Find("InputField_MsgToClient").GetComponent<InputField>();
        text_Console = GameObject.Find("Text_Console").GetComponent<Text>();
        button_OpenServer = GameObject.Find("Button_OpenServer").GetComponent<Button>();
        button_CloseServer = GameObject.Find("Button_CloseServer").GetComponent<Button>();
        button_SendDataToClient = GameObject.Find("Button_SendDataToClient").GetComponent<Button>();
        button_ClearConsole = GameObject.Find("Button_ClearConsole").GetComponent<Button>();
        toggle_ServerState = GameObject.Find("Toggle_ServerState").GetComponent<Toggle>();
    }
}
