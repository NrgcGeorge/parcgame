using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PlaneCrush
{
    enum Phases
    {
        ACKNOWLEDGE,
        ATACK,
        HIT,
        LOSE
    }

    class Server
    {
        String ServerIP = "192.168.0.171";
        int ServerPort = 9000;
        TcpListener serverSocket;
        TcpClient clientSocket;
        static Hashtable clientsList = new Hashtable();

        public static int readyPlayers = 0;
        static string activePlayer;
        public static Phases phase = Phases.ACKNOWLEDGE;

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
                if (clientsList.Count < 2)
                {
                    clientSocket = serverSocket.AcceptTcpClient();
                    NetworkStream networkStream = clientSocket.GetStream();
                    clientsList.Add(clientSocket.Client.RemoteEndPoint.ToString(), clientSocket);
                    activePlayer = clientSocket.Client.RemoteEndPoint.ToString();

                    msg(clientSocket.Client.RemoteEndPoint.ToString() + " has connected");
                    HandleClient client = new HandleClient(clientSocket, clientSocket.Client.RemoteEndPoint.ToString());
                }
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
                if (!client.Value.Equals(uName))
                {
                    TcpClient broadcastSocket = (TcpClient)client.Value;
                    NetworkStream broadcastStream = broadcastSocket.GetStream();

                    broadcastStream.Write(byteMsg, 0, byteMsg.Length);
                    broadcastStream.Flush();
                }
            }
        }

        public static void sendActivePlayerMsg() {
            Byte[] msg = Encoding.ASCII.GetBytes(Server.activePlayer);
            Server.broadcastMsg("server", msg);

            foreach (DictionaryEntry client in clientsList) {
                if (!client.Key.Equals(Server.activePlayer)) {
                    activePlayer = (string)client.Key;
                }
            }
        }
    }
}
