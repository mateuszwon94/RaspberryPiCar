using System;
using System.Runtime.InteropServices;

namespace RaspberryPiCar.NeoPixel.Native {
    [StructLayout(LayoutKind.Sequential)]
    internal struct ws2811_channel_t {
        public int    gpionum;
        public int    invert;
        public int    count;
        public int    strip_type;
        public IntPtr leds;
        public byte   brightness;
        public byte   wshift;
        public byte   rshift;
        public byte   gshift;
        public byte   bshift;
        public IntPtr gamma;
    }
}