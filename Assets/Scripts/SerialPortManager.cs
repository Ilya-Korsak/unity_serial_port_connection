using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using UnityEngine.Events;
using System;
using System.Threading;
using System.Reflection;

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
    [SerializeField]
    [Tooltip("If you gonna to use onDataRecive - set TRUE, if not - FALSE")]
    private bool isOnDataRecivedUsing = true;
    [Header("Serial Parser")]
    [Tooltip("Returns string after keyword")]
    public ParserElement[] elements;

    private event Action mainThreadQueuedCallbacks;
    private event Action eventsClone;
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
        Thread recieveThread = new Thread(ReceiveMessage);
        recieveThread.Start();
    }
    private int CompareCommand(string command, string word)
    {
        if (command.Length > word.Length)
        {
            return -1;
        }
        int index = -1;
        bool isMatched = true;
        int i;
        for(i=0; i<command.Length; i++)
        {
            if(word[i] != command[i])
            {
                isMatched = false;
                break;
            }
        }
        if (isMatched)
        {
            index = i;
        }
        return index;
    }
    private void ReceiveMessage()
    {
        {
            while (true)
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
                            if (selectedPort.BytesToRead > 0)
                            {
                                string serialString = selectedPort.ReadLine();
                                for (int i = 0; i < elements.Length; i++)
                                {
                                    int index = CompareCommand(elements[i].command, serialString);
                                    if (index != -1 && elements[i].isSubstring)
                                    {
                                        ParserElement pElm = elements[i];
                                        string stringToSent = serialString.Substring(index);
                                        mainThreadQueuedCallbacks += () =>
                                        {
                                            pElm.Action.Invoke(stringToSent);
                                        };
                                    }
                                    else if (index != -1 && !elements[i].isSubstring)
                                    {
                                        ParserElement pElm = elements[i];
                                        mainThreadQueuedCallbacks += () =>
                                        {
                                            pElm.Action.Invoke(serialString);
                                        };
                                    }
                                }
                                if (isOnDataRecivedUsing)
                                {
                                    mainThreadQueuedCallbacks += () => {
                                        onDataRecived.Invoke(serialString);
                                    };
                                }
                            }
                        }
                    }
                    catch
                    {
                        //SOMETHING
                    }
                }
            }
        }
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
        catch
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
        //onDataListeners = onDataRecived.GetListenerNumber();
        if ((prev_timming + portsListRefreshDelay) < next_timming)
        {
            UpdatePortList();

            //Debug.Log("+");
            if (1000 < next_timming)
            {
                next_timming = 0;
            }
            prev_timming = next_timming;
        }
        next_timming += Time.deltaTime;
        if (mainThreadQueuedCallbacks != null)
        {
            eventsClone = mainThreadQueuedCallbacks;
            mainThreadQueuedCallbacks = null;
            eventsClone.Invoke();
            eventsClone = null;
        }
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
