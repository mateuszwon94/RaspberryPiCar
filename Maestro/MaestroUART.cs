using System;
using System.IO.Ports;

namespace RaspberryPiCar.Maestro {
    /// <summary>
    /// Pololu Mini Maestro Servo Controller which use UART to communicate 
    /// </summary> 
    class MaestroUART : IDisposable {
        /// <summary>
        /// Settings to initiate Pololu Mini Maestro
        /// </summary>
        /// <param name="device">Device to which controller is plugged in</param>
        /// <param name="baudrate">Speed of UART interface</param>
        public MaestroUART(string device = "/dev/ttyS0", int baudrate = 9600) {
            serialPort_ = new SerialPort() {
                PortName     = device,
                BaudRate     = baudrate,
                DataBits     = 8,
                Parity       = Parity.None,
                StopBits     = StopBits.One,
                ReadTimeout  = -1,
                WriteTimeout = -1,
            };

            serialPort_.Open();

            for ( int i = 0; i < Channels.Length; ++i ) {
                Channels[i] = new Channel((byte)i, serialPort_);
            }
        }

        /// <summary>
        /// Returns an error code from the controller
        /// </summary>
        public int Error {
            get {
                byte[] command = { 0xAA, 0x0C, 0xA1 & 0x7F };

                serialPort_.Write(command, 0, command.Length);

                byte[] data = { 0x00, 0x00 };
                serialPort_.Read(data, 0, data.Length);

                return BitConverter.ToInt16(data, 0);
            }
        }
        
        /// <summary>
        /// Destroys controller object by cleaning it channels and closing serial port
        /// </summary>
        public void Dispose() {
            foreach ( Channel channel in Channels ) {
                channel.Dispose();
            }

            serialPort_.Close();
        }

        /// <summary>
        /// Channels to which servos are connected
        /// </summary>
        public Channel[] Channels { get; set; } = new Channel[24];

        private readonly SerialPort serialPort_;
    }
}