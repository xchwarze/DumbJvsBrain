﻿using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DirtyJvsBrain
{
    public static class DumbJvs
    {
        private const byte JVS_OP_RESET = 0xF0;
        private const byte JVS_OP_ADDRESS = 0xF1;
        private const byte JVS_SYNC_CODE = 0xE0;
        private const byte JVS_TRUE = 0x01;
        private const byte JVS_ADDR_MASTER = 0x00;
        private const byte JVS_COMMAND_REV = 0x13;

        /// <summary>
        /// Calculates JVS checksum.
        /// </summary>
        /// <param name="dest">Destination node.</param>
        /// <param name="bytes">The data.</param>
        /// <param name="length">Length</param>
        /// <returns></returns>
        private static byte CalcChecksum(int dest, byte[] bytes, int length)
        {
            var csum = dest + length + 1;

            for (var i = 0; i < length; i++)
                csum = (csum + bytes[i]) % 256;

            return (byte)csum;
        }

        /// <summary>
        /// Gets special bits for Digital.
        /// </summary>
        /// <returns>Bits for digital.</returns>
        private static byte GetSpecialBits()
        {
            byte result = 00;
            if (InputCode.PlayerOneButtons.Test || InputCode.PlayerTwoButtons.Test)
                result |= 0x80;
            return result;
        }

        /// <summary>
        /// Gets Player 1 switch data.
        /// </summary>
        /// <returns>Bits for player 1 switch data.</returns>
        private static byte GetPlayer1Controls()
        {
            byte result = 0;
            if (InputCode.PlayerOneButtons.Start)
                result |= 0x80;
            if (InputCode.PlayerOneButtons.Service)
                result |= 0x40;
            if (InputCode.PlayerOneButtons.Up)
                result |= 0x20;
            if (InputCode.PlayerOneButtons.Down)
                result |= 0x10;
            if (InputCode.PlayerOneButtons.Left)
                result |= 0x08;
            if (InputCode.PlayerOneButtons.Right)
                result |= 0x04;
            if (InputCode.PlayerOneButtons.Button1)
                result |= 0x02;
            if (InputCode.PlayerOneButtons.Button2)
                result |= 0x01;
            return result;
        }

        /// <summary>
        /// Gets Player 1 extended switch data.
        /// </summary>
        /// <returns>Bits for player 1 extended switch data.</returns>
        private static byte GetPlayer1ControlsExt()
        {
            byte result = 0;
            if (InputCode.PlayerOneButtons.Button3)
                result |= 0x80;
            if (InputCode.PlayerOneButtons.Button4)
                result |= 0x40;
            if (InputCode.PlayerOneButtons.Button5)
                result |= 0x20;
            if (InputCode.PlayerOneButtons.Button6)
                result |= 0x10;
            return result;
        }

        /// <summary>
        /// Gets Player 2 switch data.
        /// </summary>
        /// <returns>Bits for player 2 switch data.</returns>
        private static byte GetPlayer2Controls()
        {
            byte result = 0;
            if (InputCode.PlayerTwoButtons.Start)
                result |= 0x80;
            if (InputCode.PlayerTwoButtons.Service)
                result |= 0x40;
            if (InputCode.PlayerTwoButtons.Up || InputCode.ShiftUp)
                result |= 0x20;
            if (InputCode.PlayerTwoButtons.Down || InputCode.ShiftDown)
                result |= 0x10;
            if (InputCode.PlayerTwoButtons.Left)
                result |= 0x08;
            if (InputCode.PlayerTwoButtons.Right)
                result |= 0x04;
            if (InputCode.PlayerTwoButtons.Button1)
                result |= 0x02;
            if (InputCode.PlayerTwoButtons.Button2)
                result |= 0x01;
            return result;
        }

        /// <summary>
        /// Gets Player 2 extended switch data.
        /// </summary>
        /// <returns>Bits for player 2 extended switch data.</returns>
        private static byte GetPlayer2ControlsExt()
        {
            byte result = 0;
            if (InputCode.PlayerTwoButtons.Button3)
                result |= 0x80;
            if (InputCode.PlayerTwoButtons.Button4)
                result |= 0x40;
            if (InputCode.PlayerTwoButtons.Button5)
                result |= 0x20;
            if (InputCode.PlayerTwoButtons.Button6)
                result |= 0x10;
            return result;
        }

        /// <summary>
        /// THIS CODE IS BEYOND RETARTED AND FOR HACKY TESTS ONLY!!!
        /// For proper JVS handling: must code proper detection of packages, multiple requests in one package and proper responses!
        /// Now we just know what SEGA asks and return back like bunch of monkey boys.
        /// Feel free to improve.
        /// </summary>
        /// <param name="data">Input data from the com port.</param>
        /// <returns>"proper" response.</returns>
        public static  byte[] GetReply(byte[] data)
        {
            var replyBytes = new List<byte>();
            if (data.Length <= 3)
                return replyBytes.ToArray();
            // TODO: REWRITE TO UNDERSTAND MULTIPLE COMMANDS IN ONE, INSTEAD OF HARDCODED WITH LENGTH. Good for MKDX etc.
            switch (data[3])
            {
                // E0FF03F0D9CB
                case JVS_OP_RESET:
                    {
                        return replyBytes.ToArray();
                        // You can reply but it seems to be totally useless in 99% of cases.
                        //replyBytes.Add(JVS_SYNC_CODE);
                        //replyBytes.Add(0x00);
                        //replyBytes.Add(0x03);
                        //replyBytes.Add(0x01);
                        //replyBytes.Add(0x01);
                        //replyBytes.Add(0x05);
                        //return replyBytes.ToArray();
                    }
                // E0FF03F101F4
                case JVS_OP_ADDRESS:
                    {
                        var replyData = new byte[] { 0x01, 0x01 };
                        replyBytes.Add(JVS_SYNC_CODE);
                        replyBytes.Add(0x00);
                        replyBytes.Add((byte)(replyData.Length + 1));
                        replyBytes.AddRange(replyData);
                        replyBytes.Add(CalcChecksum(0x00, replyData, replyData.Length));
                        return replyBytes.ToArray();
                    }
                // E001051112131450
                // E001041112133B
                case 0x11: // (JVS_COMMAND_REV)
                    {
                        if (data.Length == 8)
                        {
                            var replyData = new byte[]
                        {
                            0x01, 0x01, 0x11, 0x01, 0x20, 0x01, 0x10, 0x01, 0x01, 0x02, 0x0D, 0x00, 0x02, 0x02, 0x00,
                            0x00, 0x03, 0x08, 0x00, 0x00, 0x12, 0x06, 0x00, 0x00, 0x00
                        };
                            replyBytes.Add(JVS_SYNC_CODE);
                            replyBytes.Add(0);
                            replyBytes.Add((byte)(replyData.Length + 1));
                            replyBytes.AddRange(replyData);
                            replyBytes.Add(CalcChecksum(0x00, replyData, replyData.Length));
                            return replyBytes.ToArray();
                        }
                        if (data.Length == 7)
                        {
                            var replyData = new byte[] { 0x01, 0x01, 0x13, 0x01, 0x20, 0x01, 0x10 };
                            replyBytes.Add(JVS_SYNC_CODE);
                            replyBytes.Add(0x00);
                            replyBytes.Add((byte)(replyData.Length + 1));
                            replyBytes.AddRange(replyData);
                            replyBytes.Add(CalcChecksum(0x00, replyData, replyData.Length));
                            return replyBytes.ToArray();
                        }
                        return replyBytes.ToArray();
                    }
                // E001021013
                case 0x10: // GET IDENTIFIER
                    {
                        //string replyIdentifier = "\x1\x1SEGA ENTERPRISES,LTD.;I/O BD JVS;837-13551 ;Ver1.00;98/10\0";
                        string replyIdentifier = "\x1\x1SEGA CORPORATION;I/O BD JVS;837-14572;Ver1.00;2005/10\0";
                        //string replyIdentifier = "\x1\x1NBGI.;NA-JV;Ver6.01;JPN,MK3100-1-NA-APR0-A01\0";
                        var replyData = new List<byte>();
                        byte[] toBytes = Encoding.ASCII.GetBytes(replyIdentifier);
                        replyData.AddRange(toBytes);
                        replyBytes.Add(JVS_SYNC_CODE);
                        replyBytes.Add(0);
                        replyBytes.Add((byte)(replyData.Count + 1));
                        replyBytes.AddRange(replyData.ToArray());
                        replyBytes.Add(CalcChecksum(0x00, replyData.ToArray(), replyData.Count));
                        return replyBytes.ToArray();
                    }
                // e0 01 02 14 17
                case 0x14: // get slave features
                    {
                        // e0 00 14 01 01 01 02 0e 00 02 02 00 00 03 08 0a 00 12 14 00 00 00 66
                        var replyData = new byte[] { 0x01, 0x01, 0x01, 0x02, 0x0E, 0x00, 0x02, 0x02, 0x00, 0x00, 0x03, 0x08, 0x0A, 0x00, 0x12, 0x14, 0x00, 0x00, 0x00 };
                        replyBytes.Add(JVS_SYNC_CODE);
                        replyBytes.Add(0);
                        replyBytes.Add((byte)(replyData.Length + 1));
                        replyBytes.AddRange(replyData);
                        replyBytes.Add(CalcChecksum(0x00, replyData.ToArray(), replyData.Length));
                        return replyBytes.ToArray();
                    }
                case 0x15:
                    {
                        var replyData = new byte[] { 0x01, 0x01, 0x05 };
                        replyBytes.Add(JVS_SYNC_CODE);
                        replyBytes.Add(0);
                        replyBytes.Add((byte)(replyData.Length + 1));
                        replyBytes.AddRange(replyData);
                        replyBytes.Add(CalcChecksum(0x00, replyData.ToArray(), replyData.Length));
                        return replyBytes.ToArray();
                    }
                // E0010D200202220821023203000000B4
                case 0x20: // JVS_GET_DIGITAL + ANALOG
                    {
                        //Console.WriteLine("data len: " + data.Length);
                        if (data.Length == 16)
                        {
                            // 01    01    00    00    00    00    00    01    71    00    1B    00    1F 00 1A 00 1F 00 1A 00 19 00 18 00 01 00 00 00 00 02
                            var replyData = new byte[] { 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x7A, 0x00, 0x1D, 0x00, 0x1E, 0x00, 0x1A, 0x00, 0x22, 0x00, 0x1C, 0x00, 0x1A, 0x00, 0x19, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x02 };
                            replyData[2] = GetSpecialBits();
                            replyData[3] = GetPlayer1Controls();
                            replyData[4] = GetPlayer1ControlsExt();
                            replyData[5] = GetPlayer2Controls();
                            replyData[6] = GetPlayer2ControlsExt();
                            replyData[8] = (byte)InputCode.Wheel;
                            replyData[10] = (byte)InputCode.Gas;
                            replyData[12] = (byte)InputCode.Brake;
                            replyBytes.Add(JVS_SYNC_CODE);
                            replyBytes.Add(0);
                            replyBytes.Add((byte)(replyData.Length + 1));
                            replyBytes.AddRange(replyData);
                            replyBytes.Add(CalcChecksum(0x00, replyData.ToArray(), replyData.Length));
                            return replyBytes.ToArray();
                        }
                        // e0010c2002022101220632020000af
                        else if (data.Length == 15)
                        {
                            //  e0 00 19 01 01 00 00 00 00 00 01 00 00 01 4f 40 4f 00 52 40 57 80 4e 40 4f 00 01 42 
                            //  01 01 00 00 00 00 00 01 00 00 01 8B C0 8C 40 90 40 97 40 8A 40 8C 80 01
                            //  01 01 00 00 00 00 00 01 00 00 01 4f 40 4f 00 52 40 57 80 4e 40 4f 00 01 42 
                            //                             01    01    00    00    00    00    00    01    00    00    01    70 00 1B 00 1F 00 1A 00 1F 00 1A 00 02
                            var replyData = new byte[] { 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x01, 0x4f, 0x40, 0x4f, 0x00, 0x52, 0x40, 0x57, 0x80, 0x4e, 0x40, 0x4f, 0x00, 0x01 };
                            replyData[2] = GetSpecialBits();
                            replyData[3] = GetPlayer1Controls();
                            replyData[4] = GetPlayer1ControlsExt();
                            replyData[5] = GetPlayer2Controls();
                            replyData[6] = GetPlayer2ControlsExt();
                            replyData[11] = (byte)InputCode.Wheel;
                            replyData[13] = (byte)InputCode.Gas;
                            replyData[15] = (byte)InputCode.Brake;
                            replyBytes.Add(JVS_SYNC_CODE);
                            replyBytes.Add(0);
                            replyBytes.Add((byte)(replyData.Length + 1));
                            replyBytes.AddRange(replyData);
                            replyBytes.Add(CalcChecksum(0x00, replyData.ToArray(), replyData.Length));
                            return replyBytes.ToArray();
                        }
                        // 0xe0, 0x01, 0x0b, 0x20, 0x02, 0x02, 0x22, 0x08, 0x21, 0x02, 0x32, 0x01, 0x00, 0xb0
                        else if (data.Length == 14)
                        {
                            // 01 01 00 00 00 00 00 01 71 00 1A 00 1F 00 1B 00 1F 00 1A 00 19 00 18 00 01 00 00 00 00 01
                            var replyData = new byte[] { 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x71, 0x00, 0x1A, 0x00, 0x1F, 0x00, 0x1B, 0x00, 0x1F, 0x00, 0x1A, 0x00, 0x19, 0x00, 0x18, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01 };
                            replyData[2] = GetSpecialBits();
                            replyData[3] = GetPlayer1Controls();
                            replyData[4] = GetPlayer1ControlsExt();
                            replyData[5] = GetPlayer2Controls();
                            replyData[6] = GetPlayer2ControlsExt();
                            replyData[8] = (byte)InputCode.Wheel;
                            replyData[10] = (byte)InputCode.Gas;
                            replyData[12] = (byte)InputCode.Brake;
                            replyBytes.Add(JVS_SYNC_CODE);
                            replyBytes.Add(0);
                            replyBytes.Add((byte)(replyData.Length + 1));
                            replyBytes.AddRange(replyData);
                            replyBytes.Add(CalcChecksum(0x00, replyData.ToArray(), replyData.Length));
                            return replyBytes.ToArray();
                        }
                        // E00108200202220821027A
                        else if (data.Length == 11)
                        {
                            //                             01    01    00    00    00    00    00    01    78 00 1B 00 1E 00 19 00 20 00 1B 00 19 00 18 00 01 00 00 00 00
                            var replyData = new byte[] { 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x80, 0x00, 0x1B, 0x00, 0x1F, 0x00, 0x1A, 0x00, 0x21, 0x00, 0x1B, 0x00, 0x19, 0x00, 0x18, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 };
                            replyData[2] = GetSpecialBits();
                            replyData[3] = GetPlayer1Controls();
                            replyData[4] = GetPlayer1ControlsExt();
                            replyData[5] = GetPlayer2Controls();
                            replyData[6] = GetPlayer2ControlsExt();
                            replyData[8] = (byte)InputCode.Wheel;
                            replyData[10] = (byte)InputCode.Gas;
                            replyData[12] = (byte)InputCode.Brake;
                            replyBytes.Add(JVS_SYNC_CODE);
                            replyBytes.Add(0);
                            replyBytes.Add((byte)(replyData.Length + 1));
                            replyBytes.AddRange(replyData);
                            replyBytes.Add(CalcChecksum(0x00, replyData.ToArray(), replyData.Length));
                            return replyBytes.ToArray();
                        }
                        else
                        {
                            var replyData = new byte[] { 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00 };
                            replyData[2] = GetSpecialBits();
                            replyData[3] = GetPlayer1Controls();
                            replyData[4] = GetPlayer1ControlsExt();
                            replyData[5] = GetPlayer2Controls();
                            replyData[6] = GetPlayer2ControlsExt();
                            replyBytes.Add(JVS_SYNC_CODE);
                            replyBytes.Add(0);
                            replyBytes.Add((byte)(replyData.Length + 1));
                            replyBytes.AddRange(replyData);
                            replyBytes.Add(CalcChecksum(0x00, replyData.ToArray(), replyData.Length));
                            return replyBytes.ToArray();
                        }
                    }
                case 0x21: // JVS_GET_COIN
                    {
                        var replyData = new byte[] { 0x02, 0x01, 0x00, 0x00, 0x00, 0x00 };
                        replyBytes.Add(JVS_SYNC_CODE);
                        replyBytes.Add(0);
                        replyBytes.Add((byte)(replyData.Length + 1));
                        replyBytes.AddRange(replyData);
                        replyBytes.Add(CalcChecksum(0x00, replyData.ToArray(), replyData.Length));
                        return replyBytes.ToArray();
                    }
                case 0x22: // JVS_GET_ANALOG
                    {
                        var replyData = new byte[] { 0x02, 0x01, 0x7A, 0x00, 0x1C, 0x00, 0x20, 0x00 };
                        replyData[2] = (byte)InputCode.Wheel;
                        replyData[4] = (byte)InputCode.Gas;
                        replyData[6] = (byte)InputCode.Brake;
                        replyBytes.Add(JVS_SYNC_CODE);
                        replyBytes.Add(0);
                        replyBytes.Add((byte)(replyData.Length + 1));
                        replyBytes.AddRange(replyData);
                        replyBytes.Add(CalcChecksum(0x00, replyData.ToArray(), replyData.Length));
                        return replyBytes.ToArray();
                    }
                case 0x70:
                    {
                        var replyData = new byte[] { 0x01, 0x01, 0x01, 0x07 };
                        replyBytes.Add(JVS_SYNC_CODE);
                        replyBytes.Add(0);
                        replyBytes.Add((byte)(replyData.Length + 1));
                        replyBytes.AddRange(replyData);
                        replyBytes.Add(CalcChecksum(0x00, replyData, replyData.Length));
                        return replyBytes.ToArray();
                    }
            }
            //switch (data.Substring(6,2))
            //{
            //    case JVS_OP_RESET:
            //        // JVS RESET
            //        return JVS_SYNC_CODE + JVS_ADDR_MASTER + "03" + JVS_TRUE;
            //        break;
            //        // JVS ADDRESS
            //    case JVS_OP_ADDRESS:
            //        return JVS_SYNC_CODE + JVS_ADDR_MASTER + "03" + JVS_TRUE + JVS_TRUE;
            //        break;
            //}
            // JVS RESET
            //if (data == "E0FF03F0D9CB")
            //{
            //    //return "010300E0";
            //    return "E0000301";
            //}
            //// JVS_ADDRESS
            //if (data == "E0FF03F101F4")
            //{
            //    return "E00003010105";
            //}
            //return "00";
            return replyBytes.ToArray();
        }
    }
}
