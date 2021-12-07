using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerialTester : MonoBehaviour
{
    // Start is called before the first frame update
    public List<string> ports;
    public void ChooseSecondCOMPort()
    {
        SerialPortManager.instance.TrySetPort(SerialPortManager.instance.availablePortNamesList[1], PortSpeed.SP9600);
    }
    public void ClosePort()
    {
        SerialPortManager.instance.TryClosePort();
    }
    public void Send9()
    {
        SerialPortManager.instance.SendData("9");
    }
    public void GetPorts(List<string> portParams)
    {
        ports = portParams;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
