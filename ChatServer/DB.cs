using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace ChatServer
{
    class DB
    {
        private SQLiteConnection con;
        public DB(string name)
        {
            con = new SQLiteConnection("URI=file:" + name + ".db");
            con.Open();

            using SQLiteCommand cmd = new SQLiteCommand(con);
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS tMessage (
            id INTEGER PRIMARY KEY,
            timeSent DATETIME NOT NULL,
            nickname VARCHAR(16) NOT NULL,
            message TEXT NOT NULL
            )";
            cmd.ExecuteNonQuery();

        }

        public Message AddMessage(string nickname, string message)
        {
            DateTime now = DateTime.Now;
            string dt = now.ToShortDateString() + " " + now.ToLongTimeString();

            using SQLiteCommand cmd = new SQLiteCommand(con);
            cmd.CommandText = "INSERT INTO tMessage (timeSent, nickname, message) VALUES(@timeSent, @nick, @msg)";
            cmd.Parameters.AddWithValue("@nick", nickname);
            cmd.Parameters.AddWithValue("@msg", message);
            cmd.Parameters.AddWithValue("@timeSent", dt);
            cmd.Prepare();

            if(cmd.ExecuteNonQuery() != 1)
            {
                throw new Exception();
            }

            return new Message(dt, nickname, message);
        }

        // Sorted oldest -> newest
        public List<Message> GetAllMessages()
        {
            List<Message> list = new List<Message>();

            using SQLiteCommand cmd = new SQLiteCommand(con);
            cmd.CommandText = "SELECT timeSent,nickname,message FROM tMessage ORDER BY id ASC";

            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Message m = new Message(rdr.GetString(0), rdr.GetString(1), rdr.GetString(2));
                list.Add(m);
            }

            return list;
        }

        public void wipe()
        {
            using SQLiteCommand cmd = new SQLiteCommand(con);
            cmd.CommandText = @"DELETE FROM tMessage;";
            cmd.ExecuteNonQuery();
        }

        public void close()
        {
            con.Close();
        }

        ~DB()
        {
            close();
        }
    }
}
