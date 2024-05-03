using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Kadder.Utils.WebServer.Http2;

namespace Kadder.Utils.WebServer.Http
{
    public class FrameHandler
    {
        private HeaderFrame _hangHeaderFrame;
        private readonly ConcurrentDictionary<UInt32, Request> _requestDict;
        private readonly ConcurrentDictionary<UInt32, Http2Stream> _streamDict;

        public FrameHandler()
        {
            _requestDict = new ConcurrentDictionary<UInt32, Request>();
            _streamDict = new ConcurrentDictionary<UInt32, Http2Stream>();
        }

        public async Task Handle(Http2Connection connection, ArraySegment<byte> buffer)
        {
            var frame = new Frame(buffer);
            switch (frame.Type)
            {
                case FrameType.SettingFrame:
                    await handleSettingFrameAsync(connection, buffer, frame);
                    return;
                case FrameType.WindowUpdateFrame:
                    handleWindowFrame(connection, buffer, frame);
                    return;
                case FrameType.PingFrame:
                    await handlePingFrameAsync(connection, buffer, frame);
                    return;
                default:
                    break;
            }

            if (!_streamDict.TryGetValue(frame.Identifier, out Http2Stream stream))
            {
                stream = new Http2Stream(connection, connection.ClientWindowSize);
                if (!_streamDict.TryAdd(frame.Identifier, stream))
                    throw new InvalidOperationException("Cannot add!");
            }

            await stream.WriteAsync(buffer);
        }

        private async Task handleSettingFrameAsync(Http2Connection connection, ArraySegment<byte> buffer, Frame frame)
        {
            var settingFrame = new SettingFrame(buffer, frame);
            if (settingFrame.AckFlag)
                return;

            if (settingFrame.BaseFrame.Length == 0)
            {
                await sendEmptySettingFrame();
                return;
            }

            connection.ClientConnectionSetting.UpdateFromBuffer(settingFrame);

            foreach (var stream in _streamDict)
                stream.Value.ClientWindowSize = connection.ClientConnectionSetting.InitialWindowSize;

            await sendSettingFrameAck();
            await sendServerSettingFrame();

            async Task sendSettingFrameAck()
            {
                var settingFrameAck = new byte[] {0, 0, 0, 4, 1, 0, 0, 0, 0};
                await connection.SendDataAsync(settingFrameAck);
            }

            async Task sendServerSettingFrame()
            {
                var serverSettingFrame = new SettingFrame(connection.ConnectionSetting);
                await connection.SendDataAsync(serverSettingFrame.ToBytes());
            }

            async Task sendEmptySettingFrame()
            {
                await connection.SendDataAsync(new byte[] {0, 0, 0, 4, 0, 0, 0, 0, 0});
            }
        }

        private void handleWindowFrame(Http2Connection connection, ArraySegment<byte> buffer, Frame frame)
        {
            var windowUpdateFrame = new WindowUpdateFrame(buffer, frame);
            if (frame.Identifier == 0)
            {
                connection.ClientWindowSize += (int) windowUpdateFrame.WindowSizeIncrement;
                return;
            }

            if (!_streamDict.TryGetValue(frame.Identifier, out Http2Stream stream))
                throw new InvalidOperationException("stream error!");
            stream.ClientWindowSize += (int) windowUpdateFrame.WindowSizeIncrement;
        }

        private async Task handlePingFrameAsync(Http2Connection connection, ArraySegment<byte> buffer, Frame frame)
        {
            if (frame.Identifier != 0)
                throw new InvalidOperationException("protocol error!");
            if (frame.Length < 8)
                throw new InvalidOperationException("frame size error!");

            var pingFrame = new PingFrame(buffer, frame);

            var ack = new PingFrame(true, pingFrame.Data);
            await connection.QueueSendDataAsync(ack.ToBytes());
        }
    }
}