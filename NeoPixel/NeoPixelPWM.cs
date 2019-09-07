using System;
using System.Collections.Generic;
using System.Drawing;
using NeoPixel.Native;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Linq;

namespace NeoPixel {
    public class NeoPixelPWM : IDisposable {
        /// <summary>
        /// Settings to initialize the WS281x controller
        /// </summary>
        /// <param name="frequency">Set frequency in Hz</param>
        /// <param name="dmaChannel">Set DMA channel to use</param>
        public NeoPixelPWM(int ledCount, int gpioPin, uint frequency = 800000, int dmaChannel = 10, bool invert = false, bool autoShow = true, StripType stripType = StripType.WS2812_STRIP) {
            Frequency = frequency;
            DMAChannel = dmaChannel;
            GPIOPin = gpioPin;
            Invert = invert;
            AutoShow = false;
            StripType = stripType;

            var ledList = new List<LED>(ledCount);
            for (int i = 0; i <= ledCount - 1; i++) {
                ledList.Add(new LED(i, this));
            }

            LEDs = new ReadOnlyCollection<LED>(ledList);

            _ws2811 = new ws2811_t {
                dmanum = DMAChannel,
                freq = Frequency,
                channel = new ws2811_channel_t[] {
                    new ws2811_channel_t() {
                        count = ledCount,
                        brightness = 255,
                        gpionum = GPIOPin,
                        invert = Convert.ToInt32(Invert),
                        strip_type = (int)stripType,
                    },
                    new ws2811_channel_t(),
                }
            };

            // _ws2811Handle = GCHandle.Alloc(_ws2811, GCHandleType.Pinned);

            var initResult = PInvoke.ws2811_init(ref _ws2811);
            if (initResult != ws2811_return_t.WS2811_SUCCESS) {
                var returnMessage = GetMessageForStatusCode(initResult);
                throw new Exception(String.Format("Error while initializing.{0}Error code: {1}{0}Message: {2}", Environment.NewLine, initResult.ToString(), returnMessage));
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
            var ledColor = LEDs.Select(x => x.GetRGBColorWIthBrightness()).ToArray();
            Marshal.Copy(ledColor, 0, _ws2811.channel[0].leds, ledColor.Count());

            var result = PInvoke.ws2811_render(ref _ws2811);
            if (result != ws2811_return_t.WS2811_SUCCESS) {
                var returnMessage = GetMessageForStatusCode(result);
                throw new Exception(String.Format("Error while rendering.{0}Error code: {1}{0}Message: {2}", Environment.NewLine, result.ToString(), returnMessage));
            }
        }

        public void Fill(Color color) {
            bool tempShow = AutoShow;
            AutoShow = false;

            foreach (LED led in LEDs) {
                led.Color = color;
            }

            AutoShow = tempShow;
            if (AutoShow) {
                Show();
            }
        }


        public void SetBrightnessForAll(float brightness) {
            bool tempShow = AutoShow;
            AutoShow = false;

            if (brightness > 1)
                brightness = 1;
            else if (brightness < 0)
                brightness = 0;

            foreach (LED led in LEDs) {
                led.Brightness = brightness;
            }

            AutoShow = tempShow;
            if (AutoShow) {
                Show();
            }
        }

        ~NeoPixelPWM() {
            Dispose();
        }

        public void Dispose() {
            foreach (LED led in LEDs) {
                led.Color = Color.Empty;
            }
            Show();

            PInvoke.ws2811_fini(ref _ws2811);
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

        public int LEDCount { get => LEDs.Count; }

        private ws2811_t _ws2811;

        private GCHandle _ws2811Handle;
    }
}