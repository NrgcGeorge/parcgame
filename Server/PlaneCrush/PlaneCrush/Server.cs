using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Wrapper;

namespace PlaneCrush
{
    class Server
    {
        String ServerIP = "192.168.0.171";
        int ServerPort = 9000;
        TcpListener serverSocket;
        TcpClient clientSocket;
        static Hashtable clientsList = new Hashtable();

        public static int readyPlayers = 0;
        static string activePlayer;

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

                    MessageWrapper mess = new MessageWrapper() { YourName = activePlayer, Phase = MessageWrapper.Phases.SENDIP};
                    Byte[] message = ObjectToByteArray(mess);
                    networkStream.Write(message, 0, message.Length);
 
                    msg(clientSocket.Client.RemoteEndPoint.ToString() + " has connected");
                    HandleClient client = new HandleClient(clientSocket, clientSocket.Client.RemoteEndPoint.ToString());
                }
            }
        }

        private static void msg(String ms) {
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
            MessageWrapper m = new MessageWrapper() {
                ActivePlayer = Server.activePlayer,
                Phase = MessageWrapper.Phases.ACKNOWLEDGE
            };

            Byte[] message = ObjectToByteArray(m);
            Server.broadcastMsg("server", message);
            msg("Active Player: " + Server.activePlayer);

            foreach (DictionaryEntry client in clientsList) {
                if (!client.Key.Equals(Server.activePlayer)) {
                    activePlayer = (string)client.Key;
                }
            }
        }

        public static byte[] ObjectToByteArray(MessageWrapper obj)
        {
            byte[] bytes;
            using (var _MemoryStream = new MemoryStream())
            {
                IFormatter _BinaryFormatter = new BinaryFormatter();
                _BinaryFormatter.Serialize(_MemoryStream, obj);
                bytes = _MemoryStream.ToArray();
            }
            return bytes;
        }
    }
}
