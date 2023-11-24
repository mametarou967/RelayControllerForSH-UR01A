# RelayControllerForSHUR01A

## 公式ページ

* https://www.deshide.com/product-details.html?pid=305088&_t=1661840748

## Specification:
Model: SH-UR01A
Main chip: Silicon Labs CP2102N
Interface: serial port
Pin Definition: COM,NO,NC
Connectors: USB Type A, 3 PIN Terminal Block
USB 2.0; compatible USB 1.1
Plug & Play after one time driver installation;

## Precautions:
If you need to use this USB controller to control AC switching, in order to protect your computer, we recommend you to use a USB isolator (ASIN:B093LKN3YH)

## OMRON G5V-1 Relay:
Wide switching power of 1 mA to 1 A.
Rated load: 0.5 A at 125 VAC; 1 A at 24 VDC
Rated carry current:2 A
Max. switching voltage:125 VAC, 60 VDC
Max. switching current:1 A

## AT Command:
Default UART Setting: Baud rate：9600 ;Stop Bits:1 ;Parity：None; Data Bits：8
1, AT ; Test Command, you will get "OK"
2, AT+CH1=1; Close the relay of channel 1
3, AT+CH1=0; Turn on the relay of channel 1
4, AT+BAUD=115200; Modify the baud rate to 115200

## Packing list:
1 X SH-UR01A USB Relay controller
1 X USB extension cable(1M/3.2FT)
1 X User Manual