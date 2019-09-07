﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace Maestro {
    class MaestroUART : IDisposable {
        public MaestroUART(string device = "/dev/ttyS0", int baudrate = 9600) {
            serialPort_ = new SerialPort() {
                PortName = device,
                BaudRate = baudrate,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                ReadTimeout = -1,
                WriteTimeout = -1,
            };

            serialPort_.Open();

            for (int i = 0; i < Channels.Length; ++i) {
                Channels[i] = new Channel((byte)i, serialPort_);
            }
        }

        public int Error {
            get {
                byte[] command = new byte[] { 0xAA, 0x0C, 0xA1 & 0x7F };

                serialPort_.Write(command, 0, command.Length);

                byte[] data = new byte[] { 0x00, 0x00 };
                serialPort_.Read(data, 0, data.Length);

                return System.BitConverter.ToInt16(data, 0);
            }
        }

        public void Dispose() {
            foreach (Channel channel in Channels) {
                channel.Dispose();
            }

            serialPort_.Close();
        }

        internal Channel[] Channels { get; set; } = new Channel[24];

        private readonly SerialPort serialPort_;

        public class Channel : IDisposable {
            public Channel(byte channel, SerialPort serialPort, int minPos = 0, int maxPos = 0) {
                No = channel;
                serialPort_ = serialPort;
                MinPosition = minPos;
                MaxPosition = maxPos;
            }

            public void Dispose() {
                Target = 0;
                Thread.Sleep(100);
            }

            public int Position {
                get {
                    byte[] command = new byte[] { 0xAA, 0x0C, 0x90 & 0x7F, No };

                    serialPort_.Write(command, 0, command.Length);

                    byte[] data = new byte[] { 0x00, 0x00 };
                    serialPort_.Read(data, 0, data.Length);

                    return System.BitConverter.ToInt16(data, 0);
                }
            }

            public int Speed {
                get {
                    return speed_;
                }
                set {
                    byte[] command = new byte[] { 0xAA, 0x0C, 0x87 & 0x7F, No, (byte)(value & 0x7F), (byte)((value >> 7) & 0x7F) };
                    serialPort_.Write(command, 0, command.Length);

                    speed_ = value;
                }
            }

            public int Acceleration {
                get {
                    return accel_;
                }
                set {
                    byte[] command = new byte[] { 0xAA, 0x0C, 0x89 & 0x7F, No, (byte)(value & 0x7F), (byte)((value >> 7) & 0x7F) };
                    serialPort_.Write(command, 0, command.Length);

                    accel_ = value;
                }
            }

            public int Target {
                get {
                    return target_;
                }
                set {
                    if (value != 0) {
                        if (MinPosition > 0 && value < MinPosition)
                            value = MinPosition;
                        if (MaxPosition > 0 && value > MaxPosition)
                            value = MaxPosition;
                    }

                    byte[] command = new byte[] { 0xAA, 0x0C, 0x84 & 0x7F, No, (byte)(value & 0x7F), (byte)((value >> 7) & 0x7F) };
                    serialPort_.Write(command, 0, command.Length);

                    target_ = value;
                }
            }

            public (int min, int max) Range {
                get {
                    return (MinPosition, MaxPosition);
                }
                set {
                    MinPosition = value.min;
                    MaxPosition = value.max;
                }
            }

            public byte No { get; }

            public int MinPosition { get; set; }

            public int MaxPosition { get; set; }

            private readonly SerialPort serialPort_;

            private int speed_;

            private int accel_;

            private int target_;
        }
    }
}
