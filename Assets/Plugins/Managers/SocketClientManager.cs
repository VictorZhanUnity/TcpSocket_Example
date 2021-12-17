using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class SocketClient : MonoBehaviour
{
    private string editString = "Hello World";

    private Socket serverSocket;
    private IPAddress serverIP;
    private int serverPort = 8080;
    private IPEndPoint ipEnd;

    private string reciveString;
    private string snedString;

    private byte[] reciveData = new byte[1024];
    private byte[] sendData = new byte[1024];

    private int reciveDataLength;
    private Thread connectThread;


    public void ConnectServer(string ip = "127.0.0.1", int port = 8080)
    {
        serverIP = IPAddress.Parse(ip);
        serverPort = port;
        ipEnd = new IPEndPoint(serverIP, serverPort);

        connectThread = new Thread(new ThreadStart(SocketReceive));
        connectThread.Start();
    }

    private void SocketConnect()
    {
        serverSocket?.Close();
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        print("Ready to Connect");

        serverSocket.Connect(ipEnd);

        reciveDataLength = serverSocket.Receive(reciveData);
        reciveString = Encoding.ASCII.GetString(reciveData, 0, reciveDataLength);
        print(reciveString);
    }

    public void SendString(string sendString)
    {
        sendData = new byte[1024];
        sendData = Encoding.ASCII.GetBytes(sendString);
        serverSocket.Send(sendData, sendData.Length, SocketFlags.None);
    }

    private void SocketReceive()
    {
        SocketConnect();
        while (true)
        {
            reciveData = new byte[1024];
            reciveDataLength = serverSocket.Receive(reciveData);
            if(reciveDataLength == 0)
            {
                SocketConnect();
                continue;
            }
            reciveString = Encoding.ASCII.GetString(reciveData, 0, reciveDataLength);
            print("From Server:" + reciveString);
        }
    }

    public void DisconnectServer()
    {
        connectThread?.Interrupt();
        connectThread?.Abort();
        serverSocket?.Close();
        print("Diconnect");
    }
}
