using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MultiPlug.Ext.Network.Sockets.Components.Utils;

namespace MultiPlug.Ext.Network.Sockets.Components.SocketClient
{
    public class MessageBuffer
    {
        class QItem
        {
            public string Id { get; set; }
            public DateTime TimeToLive { get; set; }
            public bool IsHex { get; set; }
        }


        private Queue<QItem> m_Q = new Queue<QItem>();

        private string m_MessageDirectory;

        public MessageBuffer(string theComponentGuid, string theComponentName)
        {
            m_MessageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp", theComponentName, theComponentGuid);
            Directory.CreateDirectory(m_MessageDirectory);

        }

        public void Enqueue(string theMessage, DateTime rheTimeToLive, bool isHex)
        {
            var MessageGuid = Guid.NewGuid().ToString();

            File.WriteAllText(Path.Combine(m_MessageDirectory, MessageGuid + ".txt"), theMessage);

            m_Q.Enqueue(new QItem { Id = MessageGuid, TimeToLive = rheTimeToLive, IsHex = isHex });

        }

        public byte[] Peek()
        {
            QItem peek;

            try
            {
                peek = m_Q.Peek();

                while (DateTime.Compare(DateTime.Now, peek.TimeToLive) > 0)
                {
                    m_Q.Dequeue();
                    peek = m_Q.Peek();
                }
            }
            catch (InvalidOperationException)
            {
                return new byte[0];
            }

            string FileRead = File.ReadAllText(Path.Combine(m_MessageDirectory, peek.Id + ".txt"));

            return peek.IsHex ? Text.HexStringToBytes(FileRead) : Encoding.ASCII.GetBytes(FileRead);
        }

        public void Dequeue()
        {
            m_Q.Dequeue();
        }
    }
}
