using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PlaneCrush
{
    class Server
    {
        String ServerIP = "192.168.0.171";
        int ServerPort = 9000;
        TcpListener serverSocket;
        TcpClient clientSocket;
        static Hashtable clientsList = new Hashtable();

        public void startServer()
        {
            serverSocket = new TcpListener(IPAddress.Parse(ServerIP), ServerPort);
            serverSocket.Start();
            Console.WriteLine("Server started listening on port:" + ServerPort);
            listen();
        }

        public void listen()
        {
            while (true) {
                clientSocket = serverSocket.AcceptTcpClient();
                NetworkStream networkStream = clientSocket.GetStream();
                Byte[] welcomeMsg = Encoding.ASCII.GetBytes("You are conected to the server!");

                clientsList.Add(clientSocket.Client.RemoteEndPoint.ToString(), clientSocket);
                
                msg(clientSocket.Client.RemoteEndPoint.ToString() + " has connected");
                HandleClient client = new HandleClient(clientSocket, clientSocket.Client.RemoteEndPoint.ToString());

            }
        }

        private void msg(String ms) {
            ms.Trim();
            Console.WriteLine(">> " + ms);
        }

        public static void broadcastMsg(string uName, byte[] byteMsg)
        {
            foreach (DictionaryEntry client in clientsList)
            {
                if (!client.Value.Equals(uName) && clientsList.Count > 1)
                {
                    TcpClient broadcastSocket = (TcpClient)client.Value;
                    NetworkStream broadcastStream = broadcastSocket.GetStream();

                    broadcastStream.Write(byteMsg, 0, byteMsg.Length);
                    broadcastStream.Flush();
                }
            }
        }
    }
}
