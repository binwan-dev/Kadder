using System;

namespace Kadder.Utils.WebServer.Http2
{

    public struct ContinuationFrame
    {
        // CONTINUATION Frame {
        //   Length (24),
        //   Type (8) = 0x09,

        //   Unused Flags (5),
        //   END_HEADERS Flag (1),
        //   Unused Flags (2),

        //   Reserved (1),
        //   Stream Identifier (31),

        //   Field Block Fragment (..),
        // }
        public ContinuationFrame(ArraySegment<byte> buffer, Frame baseFrame)
        {
            EndHeader = ((buffer[4] >> 2) & 0x1) == 1;
            FieldBlockFragment = buffer.Slice(9, (int) (baseFrame.Length));
        }

        public bool EndHeader { get; set; }

        public ArraySegment<byte> FieldBlockFragment { get; set; }
    }
}