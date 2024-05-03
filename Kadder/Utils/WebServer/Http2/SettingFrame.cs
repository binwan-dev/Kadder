using System;
using System.Collections.Generic;
using Kadder.Utils.WebServer.Socketing;

namespace Kadder.Utils.WebServer.Http2
{

    public struct SettingFrame
    {
        // SETTINGS Frame {
        //   Length (24),
        //   Type (8) = 0x04,
        //   Unused Flags (7),
        //   ACK Flag (1),
        //   Reserved (1),
        //   Stream Identifier (31) = 0,

        //   Setting (48) ...,
        // }

        // Setting {
        //   Identifier (16),
        //   Value (32),
        // }
        public SettingFrame(ConnectionSetting connectionSetting)
        {
            AckFlag = false;
            Settings = new Setting[6];
            Settings[0] = new Setting(ConnectionSetting.HeaderTableSize_Name, connectionSetting.HeaderTableSize);
            Settings[1] = new Setting(ConnectionSetting.EnablePush_Name, connectionSetting.EnablePush);
            Settings[2] = new Setting(ConnectionSetting.MaxConcurrentStream_Name,
                connectionSetting.MaxConcurrentStream);
            Settings[3] = new Setting(ConnectionSetting.InitialWindowSize_Name, connectionSetting.InitialWindowSize);
            Settings[4] = new Setting(ConnectionSetting.MaxFrameSize_Name, connectionSetting.MaxFrameSize);
            Settings[5] = new Setting(ConnectionSetting.MaxHeaderListSize_Name, connectionSetting.MaxHeaderListSize);

            BaseFrame = new Frame(36, FrameType.SettingFrame, 0, false);
        }

        public SettingFrame(ArraySegment<byte> buffer, Frame baseFrame)
        {
            BaseFrame = baseFrame;
            AckFlag = ((buffer[4] >> 0) & 0x1) == 1;
            if (baseFrame.Length % 6 != 0)
            {
                throw new InvalidOperationException("frame size error!");
            }

            var settingNums = baseFrame.Length / 6;
            Settings = new Setting[settingNums];
            for (var i = 0; i < Settings.Length; i++)
            {
                Settings[i] = new Setting(buffer.Slice(9 + i * 6, 6));
            }
        }

        public Frame BaseFrame { get; set; }

        public bool AckFlag { get; set; }

        public Setting[] Settings { get; set; }

        public byte[] ToBytes()
        {
            var buffer = BufferPool.Instance.ArrayPool.Rent(45);
            buffer = BaseFrame.Fill(buffer);

            buffer[4] = ByteHelper.SetByte(buffer[4], 1, AckFlag);
            var idx = 0;
            for (var i = 0; i < Settings.Length; i++)
            {
                idx = 9 + i * 6;
                var setting = Settings[i];
                buffer[idx] = (byte) (setting.Identifier >> 8);
                buffer[idx + 1] = (byte) setting.Identifier;
                buffer[idx + 2] = (byte) (setting.Value >> 24);
                buffer[idx + 3] = (byte) (setting.Value >> 16);
                buffer[idx + 4] = (byte) (setting.Value >> 8);
                buffer[idx + 5] = (byte) setting.Value;
            }

            return buffer;
        }

        public struct Setting
        {
            public Setting(short identifier, Int32 value)
            {
                Identifier = identifier;
                Value = value;
            }

            public Setting(ArraySegment<byte> buffer)
            {
                Identifier = (short) ((buffer[0] & 8) | (buffer[1] & 0xFF));
                Value = (Int32) ((buffer[2] & 0xFF) << 24 | ((buffer[3] & 0xFF) << 16) | ((buffer[4] & 0xFF) << 8) |
                                 (buffer[5] & 0xFF));
            }

            public short Identifier { get; set; }

            public Int32 Value { get; set; }
        }
    }
}