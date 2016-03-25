using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PlaneCrush
{
    class Server
    {
        String ServerIP = "127.0.0.1";
        int ServerPort = 7890;
        TcpListener serverSocket;
        TcpClient clientSocket;
        Hashtable clientsList = new Hashtable();

        public void startServer()
        {
            serverSocket = new TcpListener(IPAddress.Parse(ServerIP), ServerPort);
            serverSocket.Start();
            Console.WriteLine("Server started listening on port:" + ServerPort);
            listen();
        }

        public void listen() {
            while (true) {
                clientSocket = serverSocket.AcceptTcpClient();
                NetworkStream networkStream = clientSocket.GetStream();
                Byte [] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
                String dataFromClient;

                networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);
                dataFromClient = Encoding.ASCII.GetString(bytesFrom);

                clientsList.Add(dataFromClient, clientSocket);

                msg(dataFromClient + " has connected");
                HandleClient client = new HandleClient(clientSocket, dataFromClient, clientsList);
            }
        }

        private void msg(String ms) {
            ms.Trim();
            Console.WriteLine(">> " + ms);
        }
    }
}
