# Raspberry Pi Car

This is my project, when I'm building a car based on Raspberry pi

## Hardware that I'm using

1. Raspberry Pi 3B - as main board
2. Pololu Micro Maestro USB 18-channel - as servo driver
3. Servo DGS S04NF with Dagu RS002D wheel - as wheels
4. DFRobot Gravity Digital RGB LED - as lights
5. Logic level converter BSS138 - to convert Raspberry Pi 3.3V UART interface to Maestro 5V
6. ArduCam Sony IMX219 8MPx M12 - as camera, which is sending a view from the car to controlling device
7. Breadboard (830 holes) - to connect everything together
8. Supply module for contact plates MB102 - to power breadboard
9. Tamiya 70157 - as universal mounting board
10. A lot of wires of all kinds

## NeoPixel library setup

In order to get the NeoPixel wrapper working, you need to build the shared C library first. You need for that some packages which can be installed with command `sudo apt install scons gcc -y` The required source code can be found [here](https://github.com/jgarff/rpi_ws281x%5D%28https://github.com/jgarff/rpi_ws281x) (this is a link to the original project). Clone the provided repo somewhere and execute below commands in it. The PInvoke functionality has a [special search pattern](http://www.mono-project.com/docs/advanced/pinvoke/#library-handling) to find the required assembly. So copying the `ws2811.so` assembly to `/usr/lib` (as mentioned in the link above) should be enough.

    scons
    gcc -shared -o ws2811.so *.o
    sudo cp ws2811.so /usr/lib
