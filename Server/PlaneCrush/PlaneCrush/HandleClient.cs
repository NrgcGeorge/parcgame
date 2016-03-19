using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlaneCrush
{
    class HandleClient
    {
        private Hashtable clientsList;
        private TcpClient clientSocket;
        private string clientName;

        public HandleClient(TcpClient clientSocket, string clientName, Hashtable clientsList)
        {
            this.clientSocket = clientSocket;
            this.clientName = clientName;
            this.clientsList = clientsList;

            Thread ctThread = new Thread(listenToClient);
            ctThread.Start();
        }

        private void listenToClient()
        {
            while (true) {
                NetworkStream networkStream = clientSocket.GetStream();
                Byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
                String dataFromClient;

                networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);
                dataFromClient = Encoding.ASCII.GetString(bytesFrom);

                msg("Client - " + clientName + ": " + dataFromClient);
                broadcastMsg(clientName, dataFromClient);
            }
        }

        private void msg(String ms)
        {
            ms.Trim();
            Console.WriteLine(">> " + ms);
        }

        private void broadcastMsg(string uName, string msg)
        {
            foreach (DictionaryEntry client in clientsList)
            {
                TcpClient broadcastSocket = (TcpClient)client.Value;
                NetworkStream broadcastStream = broadcastSocket.GetStream();
                Byte[] broadcastBytes = Encoding.ASCII.GetBytes(uName + ": " + msg);

                broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                broadcastStream.Flush();
            }
        }
    }
}
