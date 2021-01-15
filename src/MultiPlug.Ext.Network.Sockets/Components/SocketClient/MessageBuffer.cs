using System;
using System.Collections.Generic;
using System.IO;

namespace MultiPlug.Ext.Network.Sockets.Components.SocketClient
{
    public class MessageBuffer
    {
        class QItem
        {
            public string Id { get; set; }
            public DateTime TimeToLive { get; set; }
        }


        private Queue<QItem> m_Q = new Queue<QItem>();

        private string m_MessageDirectory;

        public MessageBuffer(string theComponentGuid, string theComponentName)
        {
            m_MessageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp", theComponentName, theComponentGuid);
            Directory.CreateDirectory(m_MessageDirectory);

        }

        public void Enqueue(string theMessage, DateTime rheTimeToLive)
        {
            var MessageGuid = Guid.NewGuid().ToString();

            File.WriteAllText(Path.Combine(m_MessageDirectory, MessageGuid + ".txt"), theMessage);

            m_Q.Enqueue(new QItem { Id = MessageGuid, TimeToLive = rheTimeToLive });

        }

        public string Peek()
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
                return string.Empty;
            }

            return File.ReadAllText(Path.Combine(m_MessageDirectory, peek.Id + ".txt"));
        }

        public void Dequeue()
        {
            m_Q.Dequeue();
        }
    }
}
