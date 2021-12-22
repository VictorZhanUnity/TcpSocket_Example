using UnityEngine;
using UnityEngine.UI;
using Managers.TcpSocketHandler;
using Debug = Managers.DebugHandler.DebugManager;
public class Manager_SocketClient : MonoBehaviour
{
    [SerializeField] private InputField input_ServerIP, input_ServerPort, input_MsgToServer;
    [SerializeField] private Text text_Console;
    [SerializeField] private Button button_ConnectServer, button_DisconnectServer, button_SendDataToServer, button_ClearConsole;
    [SerializeField] private Toggle toggle_ConnectState;

    [ContextMenu("- FindParameters")]
    private void FindParameters()
    {
        input_ServerIP = GameObject.Find("InputField_ServerIP").GetComponent<InputField>();
        input_ServerPort = GameObject.Find("InputField_ServerPort").GetComponent<InputField>();
        input_MsgToServer = GameObject.Find("InputField_MsgToServer").GetComponent<InputField>();
        text_Console = GameObject.Find("Text_Console").GetComponent<Text>();
        button_ConnectServer = GameObject.Find("Button_ConnectServer").GetComponent<Button>();
        button_DisconnectServer = GameObject.Find("Button_DisconnectServer").GetComponent<Button>();
        button_SendDataToServer = GameObject.Find("Button_SendDataToServer").GetComponent<Button>();
        button_ClearConsole = GameObject.Find("Button_ClearConsole").GetComponent<Button>();
        toggle_ConnectState = GameObject.Find("Toggle_ConnectState").GetComponent<Toggle>();
    }

    #region {========= 初始化設定 =========}
    private void Start()
    {
        text_Console.text = "";
        SetFocus_InputField(input_ServerIP);
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
        TcpSocketClient.Instance.OnServerConnected += OnServerConnected;
        TcpSocketClient.Instance.OnServerDisconnected += OnServerDisconnected;
        TcpSocketClient.Instance.OnReciveDataFromServer += OnReciveDataFromSrver;
    }
    private void InitBtnListener()
    {
        button_ConnectServer.onClick.AddListener(ConnectToServer);
        button_DisconnectServer.onClick.AddListener(DisconnectServer);
        button_SendDataToServer.onClick.AddListener(SendDataToServer);
        button_ClearConsole.onClick.AddListener(ClearConsole);
    }
    public void OnDestroy()
    {
        Debug.onLogEvent -= OnLogEvent;
        button_ConnectServer.onClick.RemoveListener(ConnectToServer);
        button_DisconnectServer.onClick.RemoveListener(DisconnectServer);
        button_SendDataToServer.onClick.RemoveListener(SendDataToServer);
        button_ClearConsole.onClick.RemoveListener(ClearConsole);
        TcpSocketClient.Instance.OnServerConnected -= OnServerConnected;
        TcpSocketClient.Instance.OnServerDisconnected -= OnServerDisconnected;
        TcpSocketClient.Instance.OnReciveDataFromServer -= OnReciveDataFromSrver;
    }
    private void Update()
    {
        text_Console.text = Debug.LogHistory;
        if (TcpSocketClient.Instance.ReciveMessageList.Count > 0)
        {
            text_Console.text += "\n Parameters:";
            foreach (string key in TcpSocketClient.Instance.ReciveMessageList.Keys)
            {
                text_Console.text += $"\n{key}:{TcpSocketClient.Instance.ReciveMessageList[key]}";
            }
        }
        UpdateUI();
    }
    private void UpdateUI()
    {
        input_ServerIP.interactable = input_ServerPort.interactable = !TcpSocketClient.Instance.IsServerConnnected;
        button_ConnectServer.interactable = (input_ServerIP.text.Trim().Length > 0 && input_ServerPort.text.Trim().Length > 0 && !TcpSocketClient.Instance.IsServerConnnected);
        button_DisconnectServer.interactable =
            input_MsgToServer.interactable =
            toggle_ConnectState.isOn = TcpSocketClient.Instance.IsServerConnnected;
        button_SendDataToServer.interactable = input_MsgToServer.text.Length > 0;

        button_ClearConsole.interactable = (text_Console.text.Length > 0);
    }
    #endregion

    private void OnLogEvent(string msg)
    {
        //text_Console.text = Debug.LogHistory;
        // ↑ 無法藉由Thread來更新UI組件的內容，只有Main Thread能夠更新
    }
    public void ConnectToServer()
    {
        TcpSocketClient.Instance.ConnectToServer(input_ServerIP.text, int.Parse(input_ServerPort.text));
    }
    public void DisconnectServer()
    {
        TcpSocketClient.Instance.DisconnectFromServer();
    }
    public void SendDataToServer()
    {
        TcpSocketClient.Instance.SendDataToServer(input_MsgToServer.text);
        Debug.Log($"OnSendDataToClient: {input_MsgToServer.text}");
        SetFocus_InputField(input_MsgToServer);

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
    private void OnServerConnected(string ip, string port)
    {
        Debug.Log($"===== Server Connected: {ip}: {port} =====", Debug.TextColor.white);
        SetFocus_InputField(input_MsgToServer);
    }
    private void OnServerDisconnected()
    {
        Debug.Log("===== Server Disconnected =====", Debug.TextColor.white);
        Debug.LogSeparater();
        SetFocus_InputField(input_ServerIP);
        input_ServerPort.text = "";
    }
    private void OnReciveDataFromSrver(string msg)
    {
        Debug.Log($"Recived Data From Server: {msg}", Debug.TextColor.lime);
    } 
    #endregion

    
}
