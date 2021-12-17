using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SocketServer serverSocket;
    [SerializeField] private SocketClient clientSocket;


    [SerializeField] private Text txtIP, txtPort, txtMessage;
    [SerializeField] private Button btnOpen, btnClose, btnConnect, btnSend, btnDisconnect;

    void Start()
    {
        btnOpen.onClick.AddListener(delegate { OnClickBtnServer(btnOpen); });
        btnClose.onClick.AddListener(delegate { OnClickBtnServer(btnClose); });
        btnConnect.onClick.AddListener(delegate { OnClickBtnClient(btnConnect); });
        btnSend.onClick.AddListener(delegate { OnClickBtnClient(btnSend); });
            
    }

    private void OnClickBtnClient(Button targetBtn)
    {
        if (targetBtn == btnConnect)
        {
            clientSocket.ConnectServer();
        }
        else if (targetBtn == btnSend)
        {
            clientSocket.SendString(txtMessage.text);
        }
    }

    private void OnClickBtnServer(Button targetBtn)
    {
        if(targetBtn == btnOpen)
        {
            serverSocket.OpenSocket();   
        }
        else if (targetBtn == btnClose)
        {
            serverSocket.CloseSocket(); ;
        }
    }
}
