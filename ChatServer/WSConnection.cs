using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    class WSConnection
    {
        private WebSocketContext wsContext;
        private WebSocket webSocket;
        private byte[] recBuf = new byte[1024];
        public string nickname = null;

        public WSConnection(WebSocketContext ctx)
        {
            wsContext = ctx;
            webSocket = ctx.WebSocket;

            // First data sent is the nickname of every user in the chat

            var names = Program.server.getAllNicknames();
            _ = Send(names.Count.ToString());

            foreach(string n in names)
            {
                _ = Send(n);
            }

            // Then send all the messages

            var messages = Program.db.GetAllMessages();
            foreach(Message msg in messages)
            {
                _ = Send(msg.ToString());
            }
        }


        public async void StartListening()
        {
            while (true)
            {
                WebSocketReceiveResult result;
                try
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(recBuf), CancellationToken.None);
                }
                catch(Exception e)
                {
                    Program.server.ConnectionClosed(this);
                    return;
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _ = webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    Program.server.ConnectionClosed(this);
                    return;
                }
                else
                {
                    string str = System.Text.Encoding.UTF8.GetString(recBuf, 0, result.Count).Trim();
                    if (nickname == null)
                    {
                        // First message recieved is the nickname

                        nickname = str.TrimEnd('\r').TrimEnd('\n');
                        if (nickname.Length == 0)
                        {
                            _ = webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "", CancellationToken.None);
                            Program.server.ConnectionClosed(this);
                            return;
                        }
                        else
                        {
                            Program.server.Broadcast("[" + nickname + "[ joined the chat", this);
                        }
                    }
                    else
                    {
                        Message msg = Program.db.AddMessage(nickname, str);
                        Program.server.Broadcast(msg.ToString());
                    }
                }
            }
        }

        public async Task Send(string s)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(s, 0, Math.Min(s.Length, 1024));
            await webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);

        }


    }
}
