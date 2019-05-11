using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;

// From https://answers.unity.com/questions/601572/unity-talking-to-arduino-via-wifiethernet.html
public class TcpClientManager : MonoBehaviour {
    private bool socketReady = false;
    private TcpClient tcpClient;
    private NetworkStream networkStream;
    private StreamWriter streamWriter;
    private StreamReader streamReader;
    public String Host = "127.0.0.1";
    public Int32 Port = 5062;
    public bool lightStatus, connectionActive, newData;
    public string recievedData;

    void Update() {
        if (connectionActive) {
            while (networkStream.DataAvailable) {    // if new data is recieved from Arduino
                recievedData = readSocket();        // write it to a string
                newData = true;
            }
        }
    }

    private void OnDestroy() {
        closeSocket();
    }

    // Setup the server connection
    public void setupSocket() {                            // Socket setup here
        try {
            tcpClient = new TcpClient(Host, Port);
            networkStream = tcpClient.GetStream();
            streamWriter = new StreamWriter(networkStream);
            streamReader = new StreamReader(networkStream);
            socketReady = true;
            writeSocket("");
            connectionActive = true;
        } catch (Exception e) {
            Debug.Log("Socket error:" + e);                // catch any exceptions
        }
    }

    // Function to write data out
    public void writeSocket(string theLine) {
        if (!socketReady)
            return;
        String tmpString = theLine + Environment.NewLine;
        streamWriter.Write(tmpString);
        streamWriter.Flush();
    }

    // Function to read data in
    public String readSocket() {
        // Check if socket is ready to receive
        if (!socketReady) {
            return "";
        }
        // Read available data from the stream
        if (networkStream.DataAvailable) {
            // Return data using ReadLine()
            char[] output = new char[1];
            streamReader.Read(output, 0, 1);
            return output[0].ToString();
        }
        return "NoData";
    }

    // Function to close the socket
    public void closeSocket() {
        if (!socketReady)
            return;
        streamWriter.Close();
        streamReader.Close();
        tcpClient.Close();
        socketReady = false;
        connectionActive = false;
    }
}