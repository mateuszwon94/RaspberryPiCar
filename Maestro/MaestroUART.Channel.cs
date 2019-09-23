using System;
using System.IO.Ports;
using System.Threading;

namespace RaspberryPiCar.Maestro {
    /// <summary>
    /// Specific channel to communicate with servo
    /// </summary>
    public class Channel : IDisposable {
        /// <summary>
        /// Settings to initialize channel
        /// </summary>
        /// <param name="channel">Channel number</param>
        /// <param name="serialPort">Serial port used to communicate</param>
        /// <param name="minPos">Minimal position of servo</param>
        /// <param name="maxPos">Maximal position of servo</param>
        public Channel(byte channel, SerialPort serialPort, int minPos = 0, int maxPos = 0) {
            No          = channel;
            serialPort_ = serialPort;
            MinPosition = minPos;
            MaxPosition = maxPos;
        }

        /// <summary>
        /// Destroy the object by setting target to 0
        /// </summary>
        public void Dispose() {
            Target = 0;
            Thread.Sleep(100);
        }

        /// <summary>
        /// Returns a value which represents current position of servo
        /// </summary>
        public int Position {
            get {
                byte[] command = { 0xAA, 0x0C, 0x90 & 0x7F, No };

                serialPort_.Write(command, 0, command.Length);

                byte[] data = { 0x00, 0x00 };
                serialPort_.Read(data, 0, data.Length);

                return System.BitConverter.ToInt16(data, 0);
            }
        }

        /// <summary>
        /// Spinning speed of servo
        /// </summary>
        public int Speed {
            get => speed_;
            set {
                byte[] command = { 0xAA, 0x0C, 0x87 & 0x7F, No, (byte)(value & 0x7F), (byte)((value >> 7) & 0x7F) };
                serialPort_.Write(command, 0, command.Length);

                speed_ = value;
            }
        }

        /// <summary>
        /// Acceleration of servo
        /// </summary>
        public int Acceleration {
            get => accel_;
            set {
                byte[] command = { 0xAA, 0x0C, 0x89 & 0x7F, No, (byte)(value & 0x7F), (byte)((value >> 7) & 0x7F) };
                serialPort_.Write(command, 0, command.Length);

                accel_ = value;
            }
        }

        /// <summary>
        /// Target position of a servo
        /// 0 always is a neutral position
        /// </summary>
        public int Target {
            get => target_;
            set {
                if ( value != 0 ) {
                    if ( MinPosition > 0 && value < MinPosition )
                        value = MinPosition;
                    if ( MaxPosition > 0 && value > MaxPosition )
                        value = MaxPosition;
                }

                byte[] command = { 0xAA, 0x0C, 0x84 & 0x7F, No, (byte)(value & 0x7F), (byte)((value >> 7) & 0x7F) };
                serialPort_.Write(command, 0, command.Length);

                target_ = value;
            }
        }

        /// <summary>
        /// Available range of the servo
        /// </summary>
        public (int min, int max) Range {
            get => (MinPosition, MaxPosition);
            set {
                MinPosition = value.min;
                MaxPosition = value.max;
            }
        }

        /// <summary>
        /// Channel number
        /// </summary>
        public byte No { get; }

        /// <summary>
        /// Minimal position of servo
        /// </summary>
        public int MinPosition { get; set; }

        /// <summary>
        /// Maximal position of servo
        /// </summary>
        public int MaxPosition { get; set; }

        private readonly SerialPort serialPort_;

        private int speed_;

        private int accel_;

        private int target_;
    }
}
