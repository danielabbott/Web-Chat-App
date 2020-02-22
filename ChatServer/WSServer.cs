using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace ChatServer
{
    class WSServer
    {
        private HttpListener listener;
        private List<WSConnection> connections = new List<WSConnection>();
        public WSServer(int port)
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:" + port + "/");
        }

        public void StartListening()
        {
            listener.Start();
            listener.BeginGetContext(AcceptCallback, null);
        }

        private async void AcceptCallback(IAsyncResult ar)
        {

            HttpListenerContext listenerContext = listener.EndGetContext(ar);

            listener.BeginGetContext(AcceptCallback, null);

            if (listenerContext.Request.IsWebSocketRequest)
            {
                WebSocketContext ctx;
                try
                {
                    ctx = await listenerContext.AcceptWebSocketAsync(subProtocol: null);
                }
                catch (Exception e)
                {
                    listenerContext.Response.Abort();
                    return;
                }

                WSConnection conn = new WSConnection(ctx);
                connections.Add(conn);
                conn.StartListening();
                
            }
            else
            {
                listenerContext.Response.StatusCode = 400;
                listenerContext.Response.Close();
            }

        }

        public List<string> getAllNicknames()
        {
            List<string> names = new List<string>();
            foreach (WSConnection c in connections) {
                if(c.nickname != null)
                {
                    names.Add(c.nickname);
                }
            }
            return names;
        }

        public void ConnectionClosed(WSConnection c)
        {
            connections.Remove(c);
            if (c.nickname != null)
            {
                Broadcast("!" + c.nickname + "! left the chat");
            }
        }

        public void Broadcast(string s, WSConnection skip = null)
        {
            foreach(WSConnection conn in connections)
            {
                if (conn != skip)
                {
                    _ = conn.Send(s);
                }
            }
        }
    }
}
