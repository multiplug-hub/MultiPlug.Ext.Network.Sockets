
namespace MultiPlug.Ext.Network.Sockets.Components.Utils
{
    static internal class ControlCharacters
    {
        static internal string Lookup(uint theChar)
        {
            switch (theChar)
            {
                case 0:
                    return "[NUL]";
                case 1:
                    return "[SOH]";
                case 2:
                    return "[STX]";
                case 3:
                    return "[ETX]";
                case 4:
                    return "[EOT]";
                case 5:
                    return "[ENQ]";
                case 6:
                    return "[ACK]";
                case 7:
                    return "[BEL]";
                case 8:
                    return "[BS]";
                case 9:
                    return "[TAB]";
                case 10:
                    return "[LF]";
                case 11:
                    return "[VT]";
                case 12:
                    return "[FF]";
                case 13:
                    return "[CR]";
                case 14:
                    return "[SO]";
                case 15:
                    return "[SI]";
                case 16:
                    return "[DLE]";
                case 17:
                    return "[DC1]";
                case 18:
                    return "[DC2]";
                case 19:
                    return "[DC3]";
                case 20:
                    return "[DC4]";
                case 21:
                    return "[NAK]";
                case 22:
                    return "[SYN]";
                case 23:
                    return "[ETB]";
                case 24:
                    return "[CAN]";
                case 25:
                    return "[EM]";
                case 26:
                    return "[SUB]";
                case 27:
                    return "[ESC]";
                case 28:
                    return "[FS]";
                case 29:
                    return "[GS]";
                case 30:
                    return "[RS]";
                case 31:
                    return "[US]";
                case 127:
                    return "[DEL]";
                default:
                    return "[" + theChar.ToString() + "]";
            }
        }
    }
}
