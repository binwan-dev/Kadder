using System;
using System.Collections.Generic;
using System.IO;

namespace Kadder.Utils.WebServer.Http2
{
    public class HeaderFrame
    {
        // HEADERS Frame {
        //      Length (24),
        // 	Type (8) = 0x01,

        // 	Unused Flags (2),
        // 	PRIORITY Flag (1),
        // 	Unused Flag (1),
        // 	PADDED Flag (1),
        // 	END_HEADERS Flag (1),
        // 	Unused Flag (1),
        // 	END_STREAM Flag (1),

        // 	Reserved (1),
        // 	Stream Identifier (31),

        // 	[Pad Length (8)],
        // 	[Exclusive (1)],
        // 	[Stream Dependency (31)],
        // 	[Weight (8)],
        // 	Field Block Fragment (..),
        // 	Padding (..2040),
        // }
        public HeaderFrame(ArraySegment<byte> buffer,Frame baseFrame)
        {
            BaseFrame = baseFrame;

            PriorityFlag = ((buffer[4] >> 5) & 0x1) == 1;
            PaddedFlag = ((buffer[4] >> 3) & 0x1) == 1;
            EndHeader = ((buffer[4] >> 2) & 0x1) == 1;
            EndStream = ((buffer[4] >> 0) & 0x1) == 1;
            PadLength = buffer[9];
            Exclusive = ((buffer[10] >> 7) & 0x1) == 1;

            var first = buffer[10] >= 128 ? buffer[10] ^ 128 : buffer[10];
            StreamDependency = (uint)((first & 0xFF) << 24 | ((buffer[11] & 0xFF) << 16) | ((buffer[12] & 0xFF) << 8) | (buffer[13] & 0xFF));
            Weight = buffer[14];

	    HeaderBlockFragment = new MemoryStream();
            HeaderBlockFragment.Write(buffer.Slice(15, (int)(baseFrame.Length - 6)));

            var paddingLen = buffer.Count - baseFrame.Length - 9;
            if (paddingLen > 0)
		Padding = buffer.Slice(buffer.Count - (int) paddingLen).ToArray();
            else
		Padding = new byte[0];
        }

	public Frame BaseFrame{ get;set; }

        public bool PriorityFlag{ get;set; }

	public bool PaddedFlag{ get;set; }

	public bool EndHeader{ get;set; }

	public bool EndStream{ get;set; }

	public byte PadLength{ get;set; }

	public bool Exclusive{ get;set; }

	public UInt32 StreamDependency{ get;set; }

	public byte Weight{ get;set; }

	public MemoryStream HeaderBlockFragment { get; set; }

	public byte[] Padding { get; set; }

        public void UpdateForContinuationFrame(ArraySegment<byte> buffer, Frame baseFrame)
        {
	    EndHeader = ((buffer[4] >> 2) & 0x1) == 1;
            HeaderBlockFragment.Write(buffer.Slice(9, (int)(baseFrame.Length)));
        }
    }
}
