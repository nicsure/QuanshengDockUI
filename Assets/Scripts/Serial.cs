using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System;
using System.Text;

public static class Serial
{
    private static SerialPort serial = null;
    //private static readonly byte[] zeroLen = Array.Empty<byte>();
    private static readonly byte[] xor_array = { 0x16, 0x6c, 0x14, 0xe6, 0x2e, 0x91, 0x0d, 0x40, 0x21, 0x35, 0xd5, 0x40, 0x13, 0x03, 0xe9, 0x80 };
    private static readonly byte[] buffer = new byte[100];
    public static string[] Names => SerialPort.GetPortNames();
    public static string Port 
    { 
        get => port;
        set
        {
            port = value;
            PlayerPrefs.SetString("serialport", value);
            try { serial?.Close(); } catch { }
            try { serial?.Dispose(); } catch { }
        }
    }
    private static string port = PlayerPrefs.GetString("serialport", string.Empty);

    static Serial()
    {
        _ = Loop();
    }

    private static byte Crypt(int byt, int xori) => (byte)(byt ^ xor_array[xori & 15]);
    public static int Crc16(int byt, int crc)
    {
        crc ^= byt << 8;
        for (int i = 0; i < 8; i++)
        {
            crc <<= 1;
            if (crc > 0xffff)
            {
                crc ^= 0x1021;
                crc &= 0xffff;
            }
        }
        return crc;
    }

    private static async Task Loop() //
    {
        while (true)
        {
            try { serial?.Close(); } catch { }
            serial?.Dispose();
            try 
            {
                serial = new(port, 38400, Parity.None, 8, StopBits.One)
                {
                    ReadTimeout = 5000
                };
                serial.Open();
                SendHello();
            }
            catch
            {
                await Task.Delay(1000);
                continue;
            }
            while(true)
            {
                using var bt = Task.Run(() => GetBytes2(serial));
                int br = await bt;
                if (br < 0) break;
                for (int i = 0; i < br; i++)
                    ProcessByte(buffer[i]);
            }
        }
    }

    private static int GetBytes2(SerialPort serial)
    {
        int br;
        try
        {
            br = serial.Read(buffer, 0, 100);
        }
        catch { br = -1; }
        return br;
    }

    private enum Stage { Idle, CD, LenLSB, LenMSB, Data, CrcLSB, CrcMSB, DC, BA, UiType, UiVal1, UiVal2, UiVal3, UiDataLen, UiData }
    private static Stage stage = Stage.Idle;
    private static int pLen, pCnt;
    private static byte[] data = Array.Empty<byte>();
    private static int uiType, uiVal1, uiVal2, uiVal3, uiDataLen, uiDataCnt;
    private static byte[] uiData;
    private static void ProcessByte(int b)
    {
        switch (stage)
        {
            case Stage.Idle:
                if (b == 0xAB)
                    stage = Stage.CD;
                else
                if (b == 0xB5)
                    stage = Stage.UiType;
                break;
            case Stage.CD:
                stage = (b == 0xcd ? Stage.LenLSB : Stage.Idle);
                break;
            case Stage.LenLSB:
                pLen = b;
                stage = Stage.LenMSB;
                break;
            case Stage.LenMSB:
                pCnt = 0;
                pLen |= b << 8;
                data = new byte[pLen];
                stage = Stage.Data;
                break;
            case Stage.Data:
                data[pCnt] = Crypt(b, pCnt++);
                if (pCnt >= pLen)
                    stage = Stage.CrcLSB;
                break;
            case Stage.CrcLSB:
                stage = Stage.CrcMSB;
                break;
            case Stage.CrcMSB:
                stage = Stage.DC;
                break;
            case Stage.DC:
                stage = (b == 0xdc ? Stage.BA : Stage.Idle);
                break;
            case Stage.BA:
                stage = Stage.Idle;
                if (b == 0xba)
                    ParsePacket(data);
                break;
            case Stage.UiType:
                uiType = b;
                stage = Stage.UiVal1;
                break;
            case Stage.UiVal1:
                uiVal1 = b;
                stage = Stage.UiVal2;
                break;
            case Stage.UiVal2:
                uiVal2 = b;
                stage = Stage.UiVal3;
                break;
            case Stage.UiVal3:
                uiVal3 = b;
                stage = Stage.UiDataLen;
                break;
            case Stage.UiDataLen:
                uiDataLen = b;
                uiDataCnt = 0;
                uiData = new byte[b];
                if (b == 0)
                {
                    UiPacket(uiType, uiVal1, uiVal2, uiVal3, uiDataLen, uiData);
                    stage = Stage.Idle;
                }
                else
                    stage = Stage.UiData;
                break;
            case Stage.UiData:
                uiData[uiDataCnt++] = (byte)b;
                if (uiDataCnt >= uiDataLen)
                {
                    UiPacket(uiType, uiVal1, uiVal2, uiVal3, uiDataLen, uiData);
                    stage = Stage.Idle;
                }
                break;
        }
    }

    private static void ParsePacket(byte[] b)
    {

    }

    private static void UiPacket(int type, int val1, int val2, int val3, int dataLen, byte[] data)
    {
        switch (type)
        {
            case 0:
                while (val1 > 128) { val2++; val1 -= 128; }
                LCD.DrawText(val1, val2 + 1, 1.5, Encoding.ASCII.GetString(data));
                break;
            case 1:
                while (val1 > 128) { val2++; val1 -= 128; }
                LCD.DrawText(val1, val2 + 1, val3 / 6.0, Encoding.ASCII.GetString(data), false, false);
                break;
            case 2:
                while (val1 > 128) { val2++; val1 -= 128; }
                LCD.DrawText(val1, val2 + 1, val3 / 6.0, Encoding.ASCII.GetString(data), true, true);
                break;
            case 3:
                while (val1 > 128) { val2++; val1 -= 128; }
                LCD.DrawText(val1, val2 + 1, 2, Encoding.ASCII.GetString(data), false, true);
                break;
            case 5:
                LCD.ClearLines(val1, val2);
                break;
            case 6:
                string ps = string.Empty;
                switch (val1 & 7)
                {
                    case 1:
                        ps = "T";
                        if (MainInterface.Led != null) MainInterface.Led.color = Color.red;
                        break;
                    case 2:
                        ps = "R";
                        if (MainInterface.Led != null) MainInterface.Led.color = Color.green;
                        break;
                    case 4:
                        ps = "PS";
                        if (MainInterface.Led != null) MainInterface.Led.color = Color.black;
                        break;
                    default:
                        if (MainInterface.Led != null) MainInterface.Led.color = Color.black;
                        break;
                }
                LCD.DrawText(0, 0, 0.5, ps);
                if ((val1 & 8) != 0)
                    LCD.DrawText(8, 0, 0.5, "NOA");
                if ((val1 & 16) != 0)
                    LCD.DrawText(19, 0, 0.5, "DTMF");
                if ((val1 & 32) != 0)
                    LCD.DrawText(33, 0, 0.5, "FM");
                if (val3 != 0)
                    LCD.DrawText(42, 0, 0.5, ((char)val3).ToString());
                if ((val1 & 64) != 0)
                    LCD.DrawText(48, 0, 0.5, "<-");
                if ((val1 & 128) != 0)
                    LCD.DrawText(56, 0, 0.5, "DWR");
                if ((val2 & 1) != 0)
                    LCD.DrawText(56, 0, 0.5, "><");
                if ((val2 & 2) != 0)
                    LCD.DrawText(56, 0, 0.5, "XB");
                if ((val2 & 4) != 0)
                    LCD.DrawText(68, 0, 0.5, "VOX");
                if ((val2 & 8) != 0)
                    LCD.DrawText(82, 0, 0.5, "LK");
                if ((val2 & 16) != 0)
                    LCD.DrawText(77, 0, 0.6, "F");
                if ((val2 & 32) != 0)
                    LCD.DrawText(90, 0, 0.5, "CH");
                LCD.DrawText(93, 0, 0.5, "🔋");
                float bat = dataLen * 0.04f;
                LCD.DrawText(99, 0, 0.5, $"{bat:F2}V {(dataLen / 2.1f):F0}%");
                LCD.StatusDrawn();
                break;
            case 7:
                LCD.DrawText(0, val1, 1, val2 == 0 ? "▻" : "➤", false, true);
                break;
            case 8:
                LCD.DrawSignal(val1, val2);
                break;
        }
    }

    public static void SendHello()
    {
        SendCommand(Packet.Hello, (uint)0x12345678);
    }

    public static void SendCommand(ushort cmd, params object[] args)
    {
        var data = new byte[256];
        data[0] = 0xAB;
        data[1] = 0xCD;
        data[4] = cmd.Byte(0);
        data[5] = cmd.Byte(1);
        int ind = 8;
        foreach (object val in args)
        {
            if (val is uint[] ia)
            {
                foreach (uint u in ia)
                {
                    Array.Copy(BitConverter.GetBytes(u), 0, data, ind, 4);
                    ind += 4;
                }
            }
            else
            if (val is byte[] ba)
            {
                foreach (byte byt in ba)
                    data[ind++] = byt;
            }
            else
            if (val is byte b)
                data[ind++] = b;
            else if (val is ushort s1)
            {
                data[ind++] = s1.Byte(0);
                data[ind++] = s1.Byte(1);
            }
            else if (val is short s2)
            {
                data[ind++] = s2.Byte(0);
                data[ind++] = s2.Byte(1);
            }
            else if (val is uint i1)
            {
                data[ind++] = i1.Byte(0);
                data[ind++] = i1.Byte(1);
                data[ind++] = i1.Byte(2);
                data[ind++] = i1.Byte(3);
            }
            else if (val is int i2)
            {
                data[ind++] = i2.Byte(0);
                data[ind++] = i2.Byte(1);
                data[ind++] = i2.Byte(2);
                data[ind++] = i2.Byte(3);
            }
        }
        int prmLen = ind - 8;
        data[6] = prmLen.Byte(0);
        data[7] = prmLen.Byte(1);
        int crc = 0, xor = 0;
        for (int i = 4; i < ind; i++)
        {
            crc = Crc16(data[i], crc);
            data[i] = Crypt(data[i], xor++);
        }
        data[ind++] = Crypt(crc.Byte(0), xor++);
        data[ind++] = Crypt(crc.Byte(1), xor);
        data[ind++] = 0xDC;
        data[ind++] = 0xBA;
        ind -= 8;
        data[2] = ind.Byte(0);
        data[3] = ind.Byte(1);
        try { serial?.Write(data, 0, ind + 8); } catch { }
    }
    

    public static byte Byte(this ushort s, int byteIndex) => (byte)((s >> (byteIndex * 8)) & 0xff);
    public static byte Byte(this short s, int byteIndex) => (byte)((s >> (byteIndex * 8)) & 0xff);
    public static byte Byte(this int s, int byteIndex) => (byte)((s >> (byteIndex * 8)) & 0xff);
    public static byte Byte(this uint s, int byteIndex) => (byte)((s >> (byteIndex * 8)) & 0xff);



}


public static class Packet
{
    public const ushort Hello = 0x514;
    public const ushort GetRssi = 0x527;
    public const ushort KeyPress = 0x801;
    public const ushort GetScreen = 0x803;
    public const ushort Scan = 0x808;
    public const ushort ScanAdjust = 0x809;
    public const ushort ScanReply = 0x908;
    public const ushort WriteRegisters = 0x850;
    public const ushort ReadRegisters = 0x851;
    public const ushort RegisterInfo = 0x951;
    public const ushort WriteGPIO = 0x860;
    public const ushort ReadGPIO = 0x861;
    public const ushort GPIOInfo = 0x961;
    public const ushort GPIOPulse = 0x862;
    public const ushort EnterHardwareMode = 0x0870;
    public const ushort ExitHardwareMode = 0x0871;
    public const ushort SetReportReg = 0x0872;
    public const ushort ImHere = 0x515;
    public const ushort RssiInfo = 0x528;
    public const ushort WriteEeprom = 0x51D;
    public const ushort WriteEepromReply = 0x51E;
    public const ushort ReadEeprom = 0x51B;
    public const ushort ReadEepromReply = 0x51C;
}
