# VT105
A very basic VT105 emulation

  This is an initial build of a VT105 terminal emulator. I must emphasise that this is work in progress.
The intention behind this project is to create a terminal app that may be used with the MINC-11 system.
To read a little more about this remarkable system, I suggest you start with Sytse van Slooten's pages
at: https://pdp2011.sytse.net/wordpress/pdp-11/minc/ as he has done a lot of work in this area to
reimplement the MINC system and a VT105 terminal using an FPGA.
  This app is written in C# and is specfically built for the Windows enviroment. The reason for this
was to simplify the basic VT100 functionality which is conveniently available in the Windows console.
  The VT105 graphics have been added to the console window via an additional transparent bitmap that 
overlays the console bitmap.
The build environment is Visual Studio >=2019.
The app may be used to communicate via a COMM port or via telnet.
The telnet client code is from: https://www.codeproject.com/Articles/19071/Quick-tool-A-minimalistic-Telnet-library
See telnet.cs to swap interfaces.
Overall, the emulation is 'adequate' for playing with the MINC graphic extensions to the BASIC language.

Here is some example code for testing:


10 DIM X1(199),F1(10),Y1(199)

20 FOR I=0 TO 199

25 D1=25/(ABS(I-100)+1)

30 Y1(I)=SIN(I*PI/20)

40 Y1(I)=Y1(I)*D1

50 X1(I)=LOG10(I+1)

60 NEXT I

70 GRAPH("exact,shade,lines,vlines",,,Y1(0),1,0)

75 LABEL("BOLD","TEST GRAPH")

80 END

