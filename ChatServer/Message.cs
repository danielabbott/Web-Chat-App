using System;
using System.Collections.Generic;
using System.Text;

namespace ChatServer
{
    struct Message
    {
        public string msg;
        public string nickname;
        public string dateTime;

        public Message(string dateTime, string nickname, string msg)
        {
            this.dateTime = dateTime;
            this.nickname = nickname;
            this.msg = msg;
        }

        public string ToString()
        {
            return "<" + dateTime + " " + nickname + "> " + msg;
        }
    }
}
