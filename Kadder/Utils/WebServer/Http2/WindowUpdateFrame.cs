using System;

namespace Kadder.Utils.WebServer.Http2
{

	public struct WindowUpdateFrame
	{
		// WINDOW_UPDATE Frame {
		//   Length (24) = 0x04,
		//   Type (8) = 0x08,
		//   Unused Flags (8),
		//   Reserved (1),
		//   Stream Identifier (31),
		//   Reserved (1),
		//   Window Size Increment (31),
		// }

		public WindowUpdateFrame(ArraySegment<byte> buffer, Frame frame)
		{
			Reserved = ((buffer[9] >> 7) & 0x1) == 1;

			var first = buffer[9] >= 128 ? buffer[9] ^ 128 : buffer[9];
			WindowSizeIncrement = (UInt32) ((first & 0xFF) << 24 | ((buffer[10] & 0xFF) << 16) |
			                                ((buffer[11] & 0xFF) << 8) | (buffer[12] & 0xFF));
		}

		public bool Reserved { get; set; }

		public UInt32 WindowSizeIncrement { get; set; }
	}
}