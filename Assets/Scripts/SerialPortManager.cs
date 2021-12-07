using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using UnityEngine.Events;

public enum PortSpeed : int
{
    SP300 = 300,
    SP1200 = 1200,
    SP2400 = 2400,
    SP4800 = 4800, 
    SP9600 = 9600,
    SP19200 = 19200,
    SP38400 = 38400,
    SP57600 = 57600,
    SP115200 = 115200,
    SP230400 = 230400,
    SP250000 = 250000,
    SP500000 = 500000,
    SP1000000 = 1000000,
    SP2000000 = 2000000
}
public class SerialPortManager : MonoBehaviour
{
    public static SerialPortManager instance = null;
    [Header("Serial variables")]
    private SerialPort selectedPort = null;
    public List<string> availablePortNamesList;
    public string choosedPortName = "";
    public float portsListRefreshDelay = 1.0f;
    private float prev_timming;
    private float next_timming;
    [Header("Com port events")]
    public UnityEvent<List<string>> onPortsUpdated;
    public UnityEvent<string> onDataRecived;
    private bool CompareLists(List<string> list1, List<string> list2)
    {
        if (list1.Count != list2.Count)
        {
            return false;
        }
        for (int i = 0; i < list1.Count; i++)
        {
            if (!list1[i].Equals(list2[i]))
                return false;
        }
        return true;
    }
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance == this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        InitializeManager();
    }
    private void InitializeManager()
    {
        prev_timming = Time.deltaTime;
        next_timming = Time.deltaTime;
        availablePortNamesList = new List<string>();
    }
    public void TrySetPort(string portName, PortSpeed baudRate)
    {
        try
        {
            if (selectedPort != null)
            {
                TryClosePort();
            }
            choosedPortName = portName;
            selectedPort = new SerialPort("\\\\.\\" + portName, ((int)baudRate));
            if (!selectedPort.IsOpen)
            {
                print("Opening " + portName + ", baud " + (int)baudRate);
                selectedPort.Open();
                selectedPort.ReadTimeout = 10;
                selectedPort.Handshake = Handshake.None;
                if (selectedPort.IsOpen) { print("Open"); }
            }
        }
        catch( e)
        {
            Debug.Log("Port is not opend");
        }
    }
    public void TryClosePort()
    {
        try
        {
            choosedPortName = "";
            selectedPort.Dispose();
            selectedPort = null;
        }
        catch
        {
            //SOMETHING
        }
    }
    public void UpdatePortList()
    {
        List<string> portNamesListTmp = new List<string>();
        foreach (string serialPort in SerialPort.GetPortNames())
        {
            portNamesListTmp.Add(serialPort);
            //print("Port available: " + serialPort);
        }
        if(!CompareLists(portNamesListTmp, availablePortNamesList))
        {
            // Debug.Log("+-+-+-+-+-++--");
            onPortsUpdated.Invoke(portNamesListTmp);
        }
        availablePortNamesList = portNamesListTmp;
    }
    public void SendData(string data)
    {
        if (selectedPort != null)
        {
            try
            {
                if (!selectedPort.IsOpen)
                {
                    selectedPort.Open();
                    print("opened sp");
                }
                if (selectedPort.IsOpen)
                {
                    selectedPort.WriteLine(data);
                }
            }
            catch
            {
                //SOMETHING
            }
        }
    }
    void Update()
    {

        if((prev_timming + portsListRefreshDelay) < next_timming)
        {
            UpdatePortList();

            //Debug.Log("+");
            if (1000 < next_timming)
            {
                next_timming = 0;
            }
            prev_timming = next_timming;
        }
        if (selectedPort != null )
        {
            try
            {
                if (!selectedPort.IsOpen)
                {
                    selectedPort.Open();
                    print("opened sp");
                }
                if (selectedPort.IsOpen)
                {
                    if (selectedPort.BytesToRead > 0)
                    {
                        string serialString = selectedPort.ReadLine();
                        // Debug.Log(serialString);
                        onDataRecived.Invoke(serialString);
                    }
                }
            }
            catch
            {
                //SOMETHING
            }
        }
        next_timming += Time.deltaTime;
    }
    private void OnApplicationQuit()
    {
        try
        {
            selectedPort.Dispose();
        }
        catch
        {
            //SOMETHING
        }
    }
}
