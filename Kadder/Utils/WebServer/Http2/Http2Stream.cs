using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Kadder.Utils.WebServer.Http2
{
    public class Http2Stream
    {
        private readonly Http2Connection _connection;
        private readonly Channel<ArraySegment<byte>> _receiveStream;
        private readonly Channel<Request> _requestStream;

        public Http2Stream(Http2Connection connection, int windowSize)
        {
            WindowSize = windowSize;
            ClientWindowSize = windowSize;
            _connection = connection;
            _receiveStream = Channel.CreateUnbounded<ArraySegment<byte>>();
            _requestStream = Channel.CreateUnbounded<Request>();

            Task.Run(receiveHandler);
            Task.Run(receiveRequestHandle);
        }

        public int ClientWindowSize { get; internal set; }

        public int WindowSize { get; internal set; }

        public ValueTask WriteAsync(ArraySegment<byte> buffer)
        {
            return _receiveStream.Writer.WriteAsync(buffer);
        }

        private async Task receiveHandler()
        {
            Request request = null;
            var parseFrameType = FrameType.HeaderFrame;
            while (true)
            {
                var buffer = await _receiveStream.Reader.ReadAsync();
                var frame = new Frame(buffer);
                if (request == null)
                    request = new Request();

                switch (parseFrameType)
                {
                    case FrameType.HeaderFrame:
                        request.HeaderFrame = parseHeaderFrame(buffer, frame);
                        parseFrameType = request.HeaderFrame.EndHeader
                            ? FrameType.DataFrame
                            : FrameType.ContinuationFrame;
                        break;
                    case FrameType.ContinuationFrame:
                        parseContinuationFrame(buffer, frame, ref request);
                        parseFrameType = request.HeaderFrame.EndHeader
                            ? FrameType.DataFrame
                            : FrameType.ContinuationFrame;
                        break;
                    case FrameType.DataFrame:
                        request.DataFrame = parseDataFrame(buffer, frame);
                        if (request.DataFrame.EndStream)
                            _receiveStream.Writer.Complete();
                        await _requestStream.Writer.WriteAsync(request);
                        break;
                }
            }

            HeaderFrame parseHeaderFrame(ArraySegment<byte> buffer, Frame frame)
            {
                if (frame.Type != FrameType.HeaderFrame)
                    throw new InvalidOperationException("protocol error!");
                return new HeaderFrame(buffer, frame);
            }

            void parseContinuationFrame(ArraySegment<byte> buffer, Frame frame, ref Request request)
            {
                if (frame.Type != FrameType.ContinuationFrame)
                    throw new InvalidOperationException("protocol error!");
                request.HeaderFrame.UpdateForContinuationFrame(buffer, frame);
            }

            DataFrame parseDataFrame(ArraySegment<byte> buffer, Frame frame)
            {
                if (frame.Type != FrameType.DataFrame)
                    throw new InvalidOperationException("Protocol error!");

                return new DataFrame(buffer, frame);
            }
        }

        private async Task receiveRequestHandle()
        {
            var decoder = new HPackDecoder();
            while (true)
            {
                var request = await _requestStream.Reader.ReadAsync();
                decoder.Decode(new ArraySegment<byte>(request.HeaderFrame.HeaderBlockFragment.ToArray()));
            }
        }
    }
}