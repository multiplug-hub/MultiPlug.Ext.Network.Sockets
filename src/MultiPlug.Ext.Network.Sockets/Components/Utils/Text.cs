using System;

namespace MultiPlug.Ext.Network.Sockets.Components.Utils
{
    internal class Text
    {
        internal static byte[] HexStringToBytes(string s)
        {
            char[] splitter = { ',', '-', ' ' };
            string[] splitS = s.Split(splitter, StringSplitOptions.RemoveEmptyEntries);

            byte[] buff = new byte[splitS.Length];

            for (int i = 0; i < splitS.Length; i++)
            {
                try
                {
                    buff[i] = Convert.ToByte(splitS[i], 16);
                }
                catch
                {
                    buff[i] = 0x00;
                }
            }
            return buff;
        }
    }
}
