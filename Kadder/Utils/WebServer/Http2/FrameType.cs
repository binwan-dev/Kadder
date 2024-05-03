namespace Kadder.Utils.WebServer.Http2
{
    public class FrameType
    {
        public const byte DataFrame = 0;
        public const byte HeaderFrame = 1;
        public const byte SettingFrame = 4;
        public const byte PingFrame = 6;
        public const byte WindowUpdateFrame = 8;
        public const byte ContinuationFrame = 9;
    }
}