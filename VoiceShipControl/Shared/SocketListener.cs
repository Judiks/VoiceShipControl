using BepInEx;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VoiceShipControl;
using VoiceShipControl.Helpers;
using VoiceShipControl.Shared;

// Get Host IP Address that is used to establish a connection
// In this case, we get one IP address of localhost that is IP : 127.0.0.1
// If a host has multiple addresses, you will get a list of addresses
public delegate void MessageReceivedEvent(string message, EventArgs e);
public delegate void ErrorReceivedEvent(string error, EventArgs e);
public class SocketListener : MonoBehaviour
{
    public static SocketListener Instance;
    private static Socket _socket;
    private static Socket _handler;
    private static IPEndPoint remoteEndPoint;
    public static int connectionPort = 5050;
    public static event MessageReceivedEvent OnMessageReceivedEvent;
    public static event ErrorReceivedEvent OnErrorReceivedEvent;
    public static bool IsServerStarted = false;
    public static bool IsWaitingMessage = false;
    public static bool IsConnectionStarted = false;
    public static TaskScheduler mainThreadContext;
    public static void InitSocketListener(bool visible = false, string name = "SocketListener")
    {
        if (Instance != null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            // add an invisible game object to the scene
            GameObject obj = new GameObject();
            if (!visible)
            {
                obj.hideFlags = HideFlags.HideAndDontSave;
            }

            DontDestroyOnLoad(obj);
            Instance = obj.AddComponent<SocketListener>();
            StartServer();

            Debug.Log("SocketListener object created");

        }
        mainThreadContext = TaskScheduler.FromCurrentSynchronizationContext();
    }

    public static void StartServer()
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, connectionPort);
        _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _socket.Bind(localEndPoint);
        Console.WriteLine($"Server listening on {localEndPoint}");
        _socket.Listen(10);
        Console.WriteLine("Waiting for a connection...");
    }

    public void Update()
    {
        if (!IsServerStarted)
        {
            if (!IsConnectionStarted && _socket != null)
            {
                IsConnectionStarted = true;
                Console.WriteLine("Connecting...");
                Instance.StartCoroutine(Connect());
            }
        }
        else
        {
            KeyCode keyCode = PluginConstants.VoiceActivationButton.Value;
            int mouseKeyCode = KeyBindingHelper.GetMouseButton(keyCode);
            if (Recognizer.IsProcessStarted &&
               (UnityInput.Current.GetKeyDown(keyCode) || UnityInput.Current.GetMouseButtonDown(mouseKeyCode)) && !IsWaitingMessage)
                //|| (!IsWaitingMessage && !PluginConstants.IsVoiceActivationButtonNeeded.Value)))
            {
                Console.WriteLine("Broadcasting started");
                Instance.StartCoroutine(Broadcasting());
            }
        }
    }


    public static IEnumerator Connect()
    {
        yield return new WaitForSeconds(0.5f);
        try
        {
            Console.WriteLine("Trying to connect");
            _handler = _socket.Accept();

            if (_handler != null)
            {
                IsServerStarted = true;
                Console.WriteLine("Socket listener started");
                remoteEndPoint = (IPEndPoint)_handler.RemoteEndPoint;
                Console.WriteLine($"Accepted connection from {remoteEndPoint}");
            }
        }
        catch (Exception e) { Console.WriteLine(e.ToString()); }
    }

    public static IEnumerator Broadcasting()
    {
        IsWaitingMessage = true;
        yield return new WaitForSeconds(0f);
        SendData("continue");
        var receiveDataThread = new Thread(OnReceiveData);
        receiveDataThread.Start();

    }

    // creating new receive data task in different thread to listen socket
    static void OnReceiveData()
    {
        Task.Run(() =>
        {
            byte[] buffer = new byte[1024];
            var bytesReceived = _handler.Receive(buffer);

            // Process the result back on the main thread
            Task.Factory.StartNew(() =>
            {
                if (bytesReceived > 0)
                {
                    var data = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
                    if (data.Contains("Error"))
                    {
                        Instance.ErrorRecivedEventTrigger(data);
                    }
                    else
                    {
                        Instance.MessageRecivedEventTrigger(data);
                    }
                }
                IsWaitingMessage = false;
            }, CancellationToken.None, TaskCreationOptions.None, mainThreadContext);
        });
    }

    public static void SendData(string data)
    {
        if (_handler == null)
        {
            Debug.Log("Socket server not started SendData failed");
            return;
        }
        try
        {
            byte[] response = Encoding.ASCII.GetBytes(data.ToString());
            _handler.Send(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    protected virtual void MessageRecivedEventTrigger(string value)
    {
        MessageReceivedEvent handler = OnMessageReceivedEvent;

        // Check if there are any subscribers to the event
        if (handler != null)
        {
            handler(value, EventArgs.Empty);
        }
    }

    protected virtual void ErrorRecivedEventTrigger(string value)
    {
        ErrorReceivedEvent handler = OnErrorReceivedEvent;
        Debug.Log(value);
        // Check if there are any subscribers to the event
        if (handler != null)
        {
            handler(value, EventArgs.Empty);
        }
    }

    public static void StopSocketListener()
    {
        SendData("stop");
        _socket = _handler = null;
        IsServerStarted = IsWaitingMessage = IsConnectionStarted = false;
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        StopSocketListener();
    }

}