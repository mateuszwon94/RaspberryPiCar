using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Device.Gpio;
using System.Device.Spi;
using OpenCvSharp;
using Maestro;
using NeoPixel;
using System.Drawing;
using Iot.Device.Ws28xx;
using Iot.Device.Graphics;

class Program {
    static void Main(string[] args) {
        Console.WriteLine("Hello World!");
        foreach (string name in SerialPort.GetPortNames()) {
            Console.WriteLine(name);
        }

        // Wheels control
        using MaestroUART controller = new MaestroUART();
        for (int i = 0; i < 4; ++i) {
            controller.Channels[i].Acceleration = 5;
            controller.Channels[i].Range = (992 * 4, 2000 * 4);

            controller.Channels[i].Target = controller.Channels[i].MinPosition;
            Thread.Sleep(1000);
            controller.Channels[i].Target = controller.Channels[i].MaxPosition;
            Thread.Sleep(1000);
            controller.Channels[i].Target = 6000;
            Thread.Sleep(1000);
        }

        // Camera control
        using VideoCapture capture = new VideoCapture(0);
        // using Window window = new Window("Camera");
        using Mat image = new Mat();
        // When the movie playback reaches end, Mat.data becomes NULL.
        while (true) {
            capture.Read(image); // same as cvQueryFrame
            if (image.Empty()) break;
            // window.ShowImage(image);
            // Cv2.WaitKey(30);
        }

        // LED control

        using NeoPixelPWM neoPixel = new NeoPixelPWM(3, 18);
        neoPixel.LEDs[0].Color = Color.Red;
        neoPixel.LEDs[1].Color = Color.Green;
        neoPixel.LEDs[2].Color = Color.Blue;

        for (float brightness = 1f; brightness >= 0; brightness -= 0.005f) {
            Thread.Sleep(10);
            neoPixel.SetBrightnessForAll(brightness);
        }

        Thread.Sleep(1000);
    }
}