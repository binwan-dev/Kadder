using System;

namespace Kadder.Utils.WebServer.Http2
{

    public class ConnectionSetting
    {
        public const short HeaderTableSize_Name = 1;
        public const short EnablePush_Name = 2;
        public const short MaxConcurrentStream_Name = 3;
        public const short InitialWindowSize_Name = 4;
        public const short MaxFrameSize_Name = 5;
        public const short MaxHeaderListSize_Name = 6;

        public ConnectionSetting()
        {
            HeaderTableSize = 4096;
            EnablePush = 0;
            MaxConcurrentStream = 100;
            InitialWindowSize = 65535;
            MaxFrameSize = 16384;
            MaxHeaderListSize = HeaderTableSize * 2;
        }

        public Int32 HeaderTableSize { get; set; }

        public Int32 EnablePush { get; set; }

        public Int32 MaxConcurrentStream { get; set; }

        public Int32 InitialWindowSize { get; set; }

        public Int32 MaxFrameSize { get; set; }

        public Int32 MaxHeaderListSize { get; set; }

        public ConnectionSetting Clone()
        {
            return new ConnectionSetting()
            {
                HeaderTableSize = HeaderTableSize,
                EnablePush = EnablePush,
                MaxConcurrentStream = MaxConcurrentStream,
                InitialWindowSize = InitialWindowSize,
                MaxFrameSize = MaxFrameSize,
                MaxHeaderListSize = MaxHeaderListSize
            };
        }

        public void UpdateFromBuffer(SettingFrame frame)
        {
            foreach (var setting in frame.Settings)
            {
                switch (setting.Identifier)
                {
                    case HeaderTableSize_Name:
                        HeaderTableSize = setting.Value;
                        break;
                    case EnablePush_Name:
                        if (setting.Value != 0 && setting.Value != 1)
                            throw new InvalidOperationException("protocol error of connection error!");
                        EnablePush = setting.Value;
                        break;
                    case MaxConcurrentStream_Name:
                        MaxConcurrentStream = setting.Value;
                        break;
                    case InitialWindowSize_Name:
                        if (setting.Value > 2147483647)
                            throw new InvalidOperationException("flow control error of connection error!");
                        InitialWindowSize = setting.Value;
                        break;
                    case MaxFrameSize_Name:
                        if (setting.Value > 16777215)
                            throw new InvalidOperationException("protocol error of connection error!");
                        MaxFrameSize = setting.Value;
                        break;
                    case MaxHeaderListSize_Name:
                        MaxHeaderListSize = setting.Value;
                        break;
                }
            }
        }
    }
}