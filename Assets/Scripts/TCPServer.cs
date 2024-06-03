using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine.UI;

public class TCPServer : MonoBehaviour
{
    CharacterController characterController;
    Thread receiveThread;
    TcpClient client;
    TcpListener server;
    bool serverUp = false;

    [SerializeField]
    int port = 5066;
    int portMin = 5000;
    int portMax = 6000;

    public Button startTCPBtn;
    public Button stopTCPBtn;
    public InputField portInputField;
    public Button confirmPortBtn;
    public Text portLogText;

    void Start()
    {
        characterController = GameObject.FindWithTag("Player").GetComponent<CharacterController>();
        SetUIInteractables();
        portInputField.text = String.Format("{0}", port);
        portLogText.text = String.Format("Port Range: {0} - {1}", portMin, portMax);
    }

    void Update()
    {

    }

    public void InitTCP()
    {
        try
        {
            server = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            server.Start();
            serverUp = true;
            Debug.Log("Server started on port " + port);

            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();

        }
        catch (Exception e)
        {
            Debug.LogError("Failed to start server: " + e.ToString());
        }
    }

    public void StopTCP()
    {
        if (!serverUp) return;
        if (client != null) client.Close();
        server.Stop();
        Debug.Log("Server stopped.");
        if (receiveThread.IsAlive) receiveThread.Abort();
        serverUp = false;
    }

    private void ReceiveData()
    {
        try
        {
            Byte[] bytes = new Byte[1024];
            while (true)
            {
                Debug.Log("Waiting for a connection...");
                client = server.AcceptTcpClient();
                Debug.Log("Client connected!");
                NetworkStream stream = client.GetStream();
                int length;
                while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    var incommingData = new byte[length];
                    Array.Copy(bytes, 0, incommingData, 0, length);
                    string clientMessage = Encoding.ASCII.GetString(incommingData);
                    Debug.Log("Received from client: " + clientMessage);
                    characterController.parseMessage(clientMessage);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error receiving data: " + e.ToString());
        }
    }

    void OnApplicationQuit()
    {
        StopTCP();
    }

    public void OnStartTCPBtnClicked()
    {
        InitTCP();
        SetUIInteractables();
    }

    public void OnStopTCPBtnClicked()
    {
        StopTCP();
        SetUIInteractables();
    }

    public void OnConfirmPortBtnClicked()
    {
        int newPort;
        bool parseResult = Int32.TryParse(portInputField.text, out newPort);
        if (parseResult && newPort >= portMin && newPort <= portMax)
        {
            port = newPort;
            portLogText.text = "Port changed to: " + port;
        }
        else
        {
            portLogText.text = String.Format("Invalid port. Port Range: {0} - {1}", portMin, portMax);
        }
    }

    private void SetUIInteractables()
    {
        startTCPBtn.interactable = !serverUp;
        stopTCPBtn.interactable = serverUp;
        portInputField.interactable = !serverUp;
        confirmPortBtn.interactable = !serverUp;
    }
}
