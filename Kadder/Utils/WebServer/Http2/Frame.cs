using System;

namespace Kadder.Utils.WebServer.Http2
{
    public struct Frame
    {
        public Frame(UInt32 length, byte type, UInt32 identifier, bool reserved)
        {
            Length = length;
            Type = type;
            Identifier = identifier;
            Reserved = reserved;
        }

        public Frame(ArraySegment<byte> buffer)
        {
            Length = (UInt32) ((buffer[0] & 16) | ((buffer[1] & 0xFF) << 8) | (buffer[2] & 0xFF));
            Type = buffer[3];
            Reserved = ((buffer[5] >> 7) & 0x1) == 1;

            var first = buffer[5] >= 128 ? buffer[5] ^ 128 : buffer[5];
            Identifier = (uint) ((first & 0xFF) << 24 | ((buffer[6] & 0xFF) << 16) | ((buffer[7] & 0xFF) << 8) |
                                 (buffer[8] & 0xFF));
        }

        public UInt32 Length { get; set; }

        public byte Type { get; set; }

        public bool Reserved { get; set; }

        public UInt32 Identifier { get; set; }

        public byte[] Fill(byte[] buffer)
        {
            buffer[0] = (byte) (Length >> 16);
            buffer[1] = (byte) (Length >> 8);
            buffer[2] = (byte) (Length);
            buffer[3] = Type;
            buffer[5] = (byte) (Identifier >> 24);
            buffer[6] = (byte) (Identifier >> 16);
            buffer[7] = (byte) (Identifier >> 8);
            buffer[8] = (byte) (Identifier);

            buffer[5] = ByteHelper.SetByte(buffer[5], 8, Reserved);
            return buffer;
        }
    }
}