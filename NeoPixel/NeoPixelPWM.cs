using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using RaspberryPiCar.NeoPixel.Native;

namespace RaspberryPiCar.NeoPixel {
    public class NeoPixelPWM : IDisposable {
        /// <summary>
        /// Settings to initialize the WS281x controller
        /// </summary>
        /// <param name="gpioPin">RaspberryPi GPIO pin to which NeoPixel chain is connected</param>
        /// <param name="frequency">Set frequency in Hz</param>
        /// <param name="dmaChannel">Set DMA channel to use</param>
        /// <param name="ledCount">Number of LEDs in chain</param>
        /// <param name="invert">If bytes should be inverted</param>
        /// <param name="autoShow">If <value>true</value> color will be displayed automatically, when <value>false</value> Show() function must be called to see the effect</param>
        /// <param name="stripType">Type of the LED chain</param>
        public NeoPixelPWM(int ledCount, int gpioPin, uint frequency = 800000, int dmaChannel = 10, bool invert = false, bool autoShow = true, StripType stripType = StripType.WS2812_STRIP) {
            Frequency  = frequency;
            DMAChannel = dmaChannel;
            GPIOPin    = gpioPin;
            Invert     = invert;
            AutoShow   = false;
            StripType  = stripType;

            var ledList = new List<LED>(ledCount);
            for ( int i = 0; i <= ledCount - 1; i++ ) {
                ledList.Add(new LED(i, this));
            }

            LEDs = new ReadOnlyCollection<LED>(ledList);

            ws2811_ = new ws2811_t {
                dmanum = DMAChannel,
                freq   = Frequency,
                channel = new[] {
                    new ws2811_channel_t {
                        count      = ledCount,
                        brightness = 255,
                        gpionum    = GPIOPin,
                        invert     = Convert.ToInt32(Invert),
                        strip_type = (int)stripType,
                    },
                    new ws2811_channel_t(),
                }
            };

            // _ws2811Handle = GCHandle.Alloc(_ws2811, GCHandleType.Pinned);

            var initResult = PInvoke.ws2811_init(ref ws2811_);
            if ( initResult != ws2811_return_t.WS2811_SUCCESS ) {
                string returnMessage = GetMessageForStatusCode(initResult);
                throw new Exception($"Error while initializing.{Environment.NewLine}Error code: {initResult.ToString()}{Environment.NewLine}Message: {returnMessage}");
            }

            AutoShow = autoShow;
        }

        /// <summary>
        /// Returns the error message for the given status code
        /// </summary>
        /// <param name="statusCode">Status code to resolve</param>
        /// <returns></returns>
        private string GetMessageForStatusCode(ws2811_return_t statusCode) {
            var strPointer = PInvoke.ws2811_get_return_t_str((int)statusCode);
            return Marshal.PtrToStringAuto(strPointer);
        }

        /// <summary>
        /// Renders the content of the channels
        /// </summary>
        public void Show() {
            int[] ledColor = LEDs.Select(x => x.GetRGBColorWIthBrightness()).ToArray();
            Marshal.Copy(ledColor, 0, ws2811_.channel[0].leds, ledColor.Count());

            var result = PInvoke.ws2811_render(ref ws2811_);
            if ( result != ws2811_return_t.WS2811_SUCCESS ) {
                string returnMessage = GetMessageForStatusCode(result);
                throw new Exception($"Error while rendering.{Environment.NewLine}Error code: {result.ToString()}{Environment.NewLine}Message: {returnMessage}");
            }
        }

        /// <summary>
        /// Fill of of the LEDs with a specified color
        /// </summary>
        /// <param name="color">Color to fill the LEDs</param>
        public void Fill(Color color) {
            bool tempShow = AutoShow;
            AutoShow = false;

            foreach ( LED led in LEDs ) {
                led.Color = color;
            }

            AutoShow = tempShow;
            if ( AutoShow ) {
                Show();
            }
        }

        /// <summary>
        /// Set Brightness for all LEDs
        /// </summary>
        /// <param name="brightness">Brightness level to set from 0.0 to 1.0</param>
        public void SetBrightnessForAll(float brightness) {
            bool tempShow = AutoShow;
            AutoShow = false;

            if ( brightness > 1 )
                brightness = 1;
            else if ( brightness < 0 )
                brightness = 0;

            foreach ( LED led in LEDs ) {
                led.Brightness = brightness;
            }

            AutoShow = tempShow;
            if ( AutoShow ) {
                Show();
            }
        }

        ~NeoPixelPWM() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the object. Turn off all LEDs.
        /// </summary>
        protected virtual void Dispose(bool disposing) {
            if ( disposing ) {
                foreach ( LED led in LEDs ) {
                    led.Color = Color.Empty;
                }

                Show();
            }

            PInvoke.ws2811_fini(ref ws2811_);
            // _ws2811Handle.Free();
        }

        /// <summary>
        /// Returns the used frequency in Hz
        /// </summary>
        public uint Frequency { get; private set; }

        /// <summary>
        /// Returns the DMA channel
        /// </summary>
        public int DMAChannel { get; private set; }

        /// <summary>
        /// Returns the GPIO pin which is connected to the LED strip
        /// </summary>
        public int GPIOPin { get; private set; }

        /// <summary>
        /// Returns a value which indicates if the signal needs to be inverted.
        /// Set to true to invert the signal (when using NPN transistor level shift).
        /// </summary>
        public bool Invert { get; private set; }

        /// <summary>
        /// Returns a value which indicates if color changes should automatically be sent to the LED chain
        /// </summary>
        public bool AutoShow { get; private set; }

        /// <summary>
        /// Returns the type of the channel.
        /// The type defines the ordering of the colors.
        /// </summary>
        public StripType StripType { get; private set; }

        /// <summary>
        /// Returns all LEDs on this channel
        /// </summary>
        public ReadOnlyCollection<LED> LEDs { get; private set; }

        /// <summary>
        /// Returns a number of LED in chain
        /// </summary>
        public int LEDCount => LEDs.Count;

        private ws2811_t ws2811_;

        private GCHandle ws2811Handle_;
    }
}