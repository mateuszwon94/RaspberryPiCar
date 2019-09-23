using System.Drawing;

namespace RaspberryPiCar.NeoPixel {
    /// <summary>
    /// Represents a LED which can be controlled by the WS281x controller
    /// </summary>
    public class LED {
        /// <summary>
        /// LED which can be controlled by the WS281x controller
        /// </summary>
        /// <param name="id">ID / index of the LED</param>
        /// <param name="parent">Parent NeoPixel chain</param>
        internal LED(int id, NeoPixelPWM parent) {
            ID      = id;
            parent_ = parent;
            Color   = Color.Empty;
        }

        /// <summary>
        /// Returns the ID / index of the LED
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// Gets or sets the color for the LED
        /// </summary>
        public Color Color {
            get => color_;
            set {
                color_ = value;
                if ( parent_.AutoShow ) {
                    parent_.Show();
                }
            }
        }

        /// <summary>
        /// Brightness of a color (A channel exactly)
        /// </summary>
        public float Brightness {
            get => Color.A / 255.0f;
            set => Color = Color.FromArgb((int)(value * 255), Color);
        }

        /// <summary>
        /// Convert internal color representation into NeoPixel
        /// </summary>
        /// <returns>NeoPixel int representation of color</returns>
        internal int GetRGBColorWIthBrightness()
            => Color.FromArgb((int)(Color.R * Brightness), (int)(Color.G * Brightness), (int)(Color.B * Brightness)).ToArgb();

        private Color color_;

        private readonly NeoPixelPWM parent_;
    }
}