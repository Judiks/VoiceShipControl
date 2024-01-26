using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using System.IO;
using System;
using System.Runtime.InteropServices;

// Get Host IP Address that is used to establish a connection
// In this case, we get one IP address of localhost that is IP : 127.0.0.1
// If a host has multiple addresses, you will get a list of addresses
public delegate void MessageReceivedEvent(string message, EventArgs e);
public delegate void ErrorReceivedEvent(string error, EventArgs e);
public class SocketListener : MonoBehaviour
{
    Socket _socket;
    Socket _handler;
    Thread _thread;
    public int connectionPort = 5050;
    public event MessageReceivedEvent OnMessageReceivedEvent;
    public event ErrorReceivedEvent OnErrorReceivedEvent;
    public bool serverStarted = false;
    public void StartServer()
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, connectionPort);
        _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _socket.Bind(localEndPoint);
        Console.WriteLine($"Server listening on {localEndPoint}");
        _socket.Listen(10);
        Console.WriteLine("Waiting for a connection...");
        while (!serverStarted)
        {
            try
            {
                Console.WriteLine("Trying to connect");
                _handler = _socket.Accept();
                Thread.Sleep(500);
                if ( _handler != null )
                {
                    serverStarted = true;
                }
            } catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
    }

    public void StartBroadcasting()
    {
        if (_handler == null)
        {
            Debug.Log("Socket server not started StartBroadcasting failed");
            return;
        }
        try
        {
            Console.WriteLine("Socket listener started");
            IPEndPoint remoteEndPoint = (IPEndPoint)_handler.RemoteEndPoint;
            Console.WriteLine($"Accepted connection from {remoteEndPoint}");

            byte[] buffer = new byte[1024];
            int bytesReceived;

            do
            {
                SendData("continue");
                bytesReceived = _handler.Receive(buffer);
                if (bytesReceived > 0)
                {
                    var data = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
                    if (data.Contains("Error"))
                    {
                        ErrorRecivedEventTrigger(data);
                    }
                    else
                    {
                        MessageRecivedEventTrigger(data);
                    }
                }
                Thread.Sleep(300);
            } while (bytesReceived > 0);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public void SendData(string data)
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

    public void Stop(SocketShutdown shutdown)
    {
        _handler.Shutdown(shutdown);
        _socket.Close();
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

        // Check if there are any subscribers to the event
        if (handler != null)
        {
            handler(value, EventArgs.Empty);
        }
    }

    void OnApplicationQuit()
    {
        SendData("stop");
        Stop(SocketShutdown.Both);
        _socket.Close();
    }
}