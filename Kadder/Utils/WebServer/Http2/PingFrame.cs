using System;
using Kadder.Utils.WebServer.Socketing;

namespace Kadder.Utils.WebServer.Http2
{

	public struct PingFrame
	{
		// PING Frame {
		//   Length (24) = 0x08,
		//   Type (8) = 0x06,
		//   Unused Flags (7),
		//   ACK Flag (1),
		//   Reserved (1),
		//   Stream Identifier (31) = 0,
		//   Opaque Data (64),
		// }

		public PingFrame(bool ack, ArraySegment<byte> data)
		{
			Ack = false;
			Data = data;

			BaseFrame = new Frame((UInt32) data.Count, FrameType.PingFrame, 0, false);
		}

		public PingFrame(ArraySegment<byte> buffer, Frame frame)
		{
			BaseFrame = frame;
			Ack = ((buffer[4] >> 0) & 0x1) == 1;
			Data = buffer.Slice(9, 8);
		}

		public Frame BaseFrame { get; set; }

		public bool Ack { get; set; }

		public ArraySegment<byte> Data { get; set; }

		public byte[] ToBytes()
		{
			var buffer = BufferPool.Instance.ArrayPool.Rent(17);
			buffer = BaseFrame.Fill(buffer);
			buffer[4] = ByteHelper.SetByte(buffer[4], 1, Ack);
			buffer[9] = Data[0];
			buffer[10] = Data[1];
			buffer[11] = Data[2];
			buffer[12] = Data[3];
			buffer[13] = Data[4];
			buffer[14] = Data[5];
			buffer[15] = Data[6];
			buffer[16] = Data[7];
			return buffer;
		}
	}
}