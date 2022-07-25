# VT105
A very basic VT105 emulation

  This is an initial build of a VT105 terminal emulator. I must emphasise that the is work in progress.
The intention behind this project is to create a terminal app that may be used with the MINC-11 system.
To read a little more about this remarkable system, I suggest you start with Sytse van Slooten's pages
at: https://pdp2011.sytse.net/wordpress/pdp-11/minc/ as he has done a lot of work in this area to
reimplement the MINC system and a VT105 terminal using an FPGA.
  This app is written in C# and is specfically built for the Windows enviroment. The reason for this
was to simplify the basic VT100 functionality whcih is conveniently available in the Windows console.
  The VT105 graphics have been added to the console window via an additional background bitmap and 
setting the console bitmap to be transparent.
The build environment is Visual Studio.
The app may be used to communicate via a COMM port or via telnet.
Overall, the emulation is 'adequate' for playing with the MINC graphic extensions to the BASIC language.

