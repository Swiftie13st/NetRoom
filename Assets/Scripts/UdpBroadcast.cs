using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net.NetworkInformation;
using TMPro;


public class UdpBroadcast : MonoBehaviour
{
    [SerializeField]
    private string My_Ip = "";
    [SerializeField]
    private int sendPort = 8001;    //发送udp的port
    [SerializeField]
    private int openPort = 8080;    //服务器连接的port
    [SerializeField]
    private TMP_InputField PortInputField;
    [SerializeField]
    private string ip_section = "127.0.0."; // + 0~255 默认网段 本机网段
    [SerializeField]
    private string sendStr;

    [SerializeField]
    private TMP_InputField ins; //自定义房间信息
    Socket socket;
    EndPoint serverEnd; //服务端
    IPEndPoint ipEnd; //服务器端口
    byte[] recvData = new byte[1024]; //接收的数据，必须为字节
    byte[] sendData = new byte[1024]; //发送的数据，必须为字节
    int recvLen; //接收的数据长度
    Thread connectThread; //连接线程
    string sendText = "Default Room Info";
    // Start is called before the first frame update
    void Start()
    {
        My_Ip = GetIp();

        ip_section = My_Ip.Remove(My_Ip.LastIndexOf('.')) + '.'; // 修改搜索网段
        

        //定义套接字类型,在主线程中定义
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1); //广播权限
        //socket.Bind(new IPEndPoint(GetIpAddress(), port));


        //开启一个线程连接，必须的，否则主线程卡死
        //connectThread = new Thread(new ThreadStart(SocketReceive));
        //connectThread.Start();
    }

    public void OnStartButtonClick() //发送udp   Host开启启用
    {
        if(ins.text != "")
        {
            sendText = ins.text;
        }
        //ipEnd = new IPEndPoint(IPAddress.Parse("127.0.0.1"), sendPort); //本地
        //ipEnd = new IPEndPoint(IPAddress.Parse(ip_section + "255"), sendPort);
        ipEnd = new IPEndPoint(IPAddress.Parse("255.255.255.255"), sendPort);//广播
        sendStr = My_Ip + "," + openPort.ToString() + "," + sendText;
        SocketSend();

        InvokeRepeating("SocketSend", 5f, 5f); //每隔5s重发

        Invoke("cancelInvoke", 100f); //延时停止
    }
    public void OnStopSendButtonClick() //停止发送udp   Host点击关闭
    {
        cancelInvoke();
    }

    void cancelInvoke()
    {
        CancelInvoke("SocketSend");
        Debug.Log("Stop sending Udp.");

    }




    void SocketSend()
    {
        //清空发送缓存
        sendData = new byte[1024];
        //数据类型转换
        sendData = Encoding.ASCII.GetBytes(sendStr);
        //发送给指定服务端
        socket.SendTo(sendData, sendData.Length, SocketFlags.None, ipEnd);
        //Debug.Log(ipEnd);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string GetIp() //获取本机IP地址
    {
        string my_ip = "";
        bool gotMyIp = false;
        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (gotMyIp)
                break;
            NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
            NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;

            if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) && item.OperationalStatus == OperationalStatus.Up)
            {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        my_ip = ip.Address.ToString();
                        gotMyIp = true;
                        break;
                    }
                }
            }
        }

        //string _myHostName = Dns.GetHostName();
        //for (int i = System.Net.Dns.GetHostEntry(_myHostName).AddressList.Length - 1; i >= 0; i--)
        //{
        //    if (System.Net.Dns.GetHostEntry(_myHostName).AddressList[i].IsIPv6LinkLocal == true)
        //    {
        //        if (i < System.Net.Dns.GetHostEntry(_myHostName).AddressList.Length - 1)
        //        {
        //            my_ip = System.Net.Dns.GetHostEntry(_myHostName).AddressList[i + 1].ToString();
        //            break;
        //        }
        //    }
        //}
        return my_ip;
    }
    public void OnPortInputChanged()
    {
        if (ushort.TryParse(PortInputField.text, out ushort port))
        {
            openPort = port;
        }
    }
    void OnApplicationQuit()
    {
        SocketQuit();
    }
    //连接关闭
    void SocketQuit()
    {
        //关闭线程
        if (connectThread != null)
        {
            connectThread.Interrupt();
            connectThread.Abort();
        }
        //最后关闭socket
        if (socket != null)
            socket.Close();
    }
}
