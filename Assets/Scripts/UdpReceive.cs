using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.UI;
using TMPro;
using System;

namespace HelloWorld
{
    public class UdpReceive : MonoBehaviour
    {
        [SerializeField]
        private string recvStr;
        [SerializeField]
        private int recvPort = 8001;
        [SerializeField]
        private HashSet<string> serverSet = new HashSet<string>(); //接收到的所有地址
        [SerializeField]
        private GameObject serverIpSelectPanel;
        [SerializeField]
        private GameObject serverIpDisplayPrefab;

        [SerializeField]
        private TMP_InputField IPInputField;
        [SerializeField]
        private TMP_InputField PortInputField;
        [SerializeField]
        private GameObject scrollPanelImg;


        Socket socket; //目标socket
        EndPoint clientEnd; //客户端
        IPEndPoint ipEnd; //侦听端口
        byte[] recvData = new byte[1024]; //接收的数据，必须为字节
        int recvLen; //接收的数据长度
        Thread connectThread; //连接线程


        //初始化
        void InitSocket()
        {
            //定义侦听端口,侦听任何IP
            ipEnd = new IPEndPoint(IPAddress.Any, recvPort);
            //定义套接字类型,在主线程中定义
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //服务端需要绑定ip
            socket.Bind(ipEnd);
            //定义客户端
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            clientEnd = (EndPoint)sender;
            print("waiting for UDP dgram");

            //开启一个线程连接，必须的，否则主线程卡死
            connectThread = new Thread(new ThreadStart(SocketReceive));
            connectThread.Start();

        }

        public void onRecvButtonClicked() //开启接收UDP
        {
            //scrollPanelImg.SetActive(true);
            InitSocket();
        }


        //服务器接收
        void SocketReceive()
        {
            //进入接收循环
            while (true)
            {
                //对data清零
                recvData = new byte[1024];
                //获取客户端，获取客户端数据，用引用给客户端赋值
                recvLen = socket.ReceiveFrom(recvData, ref clientEnd);
                print("message from: " + clientEnd.ToString()); //打印客户端信息
                                                                //输出接收到的数据
                recvStr = Encoding.ASCII.GetString(recvData, 0, recvLen);
                
                //serverList.Add(recvStr);
                //splitListStr(recvStr); // 输出到屏幕  
                
                if (!serverSet.Contains(recvStr))
                {
                    print(recvStr);
                    serverSet.Add(recvStr);
                    UnityThread.executeInUpdate(() =>
                    {
                        splitListStr(recvStr); // 输出到屏幕  
                });
                }
            }
        }

        private void splitListStr(string listStr)
        {
            print(listStr);
            //分隔开
            List<string> tempList = new List<string>(listStr.Split(','));

            Debug.Log(tempList[0] + ':' + tempList[1] + "//" + tempList[2]);

            //输出到屏幕
            GameObject bthSelect = Instantiate(serverIpDisplayPrefab);

            TMP_Text serverInfoText = bthSelect.transform.Find("Server_Info").GetComponent<TMP_Text>(); 
            serverInfoText.text = tempList[0] + ':' + tempList[1] + "//" + tempList[2];
            
            //设置父关系，自动排列
            bthSelect.transform.SetParent(serverIpSelectPanel.transform);
            bthSelect.transform.localScale = new Vector3(1f,1f,1f);


            Button btnConnect = bthSelect.transform.Find("Btn_Connect").GetComponent<Button>();

            btnConnect.onClick.AddListener(delegate ()
            {
                IPInputField.text = tempList[0];
                PortInputField.text = tempList[1];
                scrollPanelImg.SetActive(false);

            });

        }

        public void onStopRecvBtnClicked()
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
            print("disconnect");
        }


        void OnApplicationQuit()
        {
            SocketQuit();
        }
    }
}


