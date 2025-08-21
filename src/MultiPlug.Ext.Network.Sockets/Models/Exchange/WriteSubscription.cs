using System;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using MultiPlug.Base.Exchange;

namespace MultiPlug.Ext.Network.Sockets.Models.Exchange
{
    public class WriteSubscription : Subscription
    {
        private string m_WritePrefix = string.Empty;
        private string m_WriteSeparator = string.Empty;
        private string m_WriteAppend = string.Empty;

        [DataMember]
        public string WritePrefix
        {
            get
            {
                return m_WritePrefix;
            }

            set
            {
                m_WritePrefix = value;
                WritePrefixUnescaped = Regex.Unescape(value);
            }
        }

        [DataMember]
        public string WriteSeparator
        {
            get
            {
                return m_WriteSeparator;
            }

            set
            {
                m_WriteSeparator = value;
                WriteSeparatorUnescaped = Regex.Unescape(value);
            }
        }

        [DataMember]
        public string WriteAppend
        {
            get
            {
                return m_WriteAppend;
            }

            set
            {
                m_WriteAppend = value;
                WriteAppendUnescaped = Regex.Unescape(value);
            }
        }

        [DataMember]
        public bool? IsHex { get; set; }
        [DataMember]
        public bool? IgnoreData { get; set; }

        public string WritePrefixUnescaped = string.Empty;
        public string WriteSeparatorUnescaped = string.Empty;
        public string WriteAppendUnescaped = string.Empty;

        public event Action<SubscriptionEvent, WriteSubscription> WriteEvent;

        public WriteSubscription()
        {
            this.Event += OnWriteSubscriptionEvent;
            IsHex = false;
            IgnoreData = false;
        }

        private void OnWriteSubscriptionEvent(SubscriptionEvent obj)
        {
            WriteEvent?.Invoke(obj, this);
        }

        public static bool Merge(WriteSubscription theMerged, WriteSubscription theMergeFrom)
        {
            if (theMergeFrom.WritePrefix != null && theMergeFrom.WritePrefix != theMerged.WritePrefix)
            {
                theMerged.WritePrefix = theMergeFrom.WritePrefix;
            }

            if (theMergeFrom.WriteSeparator != null && theMergeFrom.WriteSeparator != theMerged.WriteSeparator)
            {
                theMerged.WriteSeparator = theMergeFrom.WriteSeparator;
            }

            if (theMergeFrom.WriteAppend != null && theMergeFrom.WriteAppend != theMerged.WriteAppend)
            {
                theMerged.WriteAppend = theMergeFrom.WriteAppend;
            }

            if (theMergeFrom.IsHex != null && theMergeFrom.IsHex != theMerged.IsHex)
            {
                theMerged.IsHex = theMergeFrom.IsHex;
            }

            if (theMergeFrom.IgnoreData != null && theMergeFrom.IgnoreData != theMerged.IgnoreData)
            {
                theMerged.IgnoreData = theMergeFrom.IgnoreData;
            }

            return Base.Exchange.Subscription.Merge(theMerged, theMergeFrom);
        }
    }
}
