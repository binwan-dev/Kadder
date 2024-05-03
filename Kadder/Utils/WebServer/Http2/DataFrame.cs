using System;
using System.IO;

namespace Kadder.Utils.WebServer.Http2
{
    public struct DataFrame
    {
        // DATA Frame {
        //   Length (24),
        //   Type (8) = 0x00,

        //   Unused Flags (4),
        //   PADDED Flag (1),
        //   Unused Flags (2),
        //   END_STREAM Flag (1),

        //   Reserved (1),
        //   Stream Identifier (31),

        //   [Pad Length (8)],
        //   Data (..),
        //   Padding (..2040),
        // }

        public DataFrame(ArraySegment<byte> buffer, Frame baseFrame)
        {
            Padded = ((buffer[4] >> 3) & 0x1) == 1;
            EndStream = ((buffer[4] >> 0) & 0x1) == 1;
            PadLength = buffer[9];

            // Data = new MemoryStream();
            Data = buffer.Slice(10, (int) (baseFrame.Length - 1)).ToArray();

            // var paddingLen = buffer.Count - baseFrame.Length - 9;
            // if (paddingLen > 0)
            //     Padding = buffer.Slice(buffer.Count - (int) paddingLen).ToArray();
            // else
            //     Padding = new byte[0];
        }

        public bool Padded { get; set; }

        public bool EndStream { get; set; }

        public byte PadLength { get; set; }

        public byte[] Data { get; set; }

        // public byte[] Padding{ get; set; }
    }
}