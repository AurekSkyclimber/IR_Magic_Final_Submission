================
Running Programs
================

Run these scripts and programs in this specific order. Close in reverse order.

1. Laptop | py -3.6 AnalyzeShape.py | close with ctrl + c after stopping Unity
2. Laptop | Open Unity project -> Hit play | Stop 3rd
3. Laptop | py -3.6 ForwardWandPosFromPiToUnity.py | Should close when stop cvImageTest.py
4. Ras Pi | python3 PiIRPosSend.py | Close with the q key

=====
Setup
=====

You will need to directly connect your Raspberry Pi and your computer with an ethernet cable.
Turn off wi-fi on Raspberry Pi to force discovery of your computer.
Run ipconfig in the command prompt on your computer
Find the ip for your ethernet connection between the computer and the Pi
Change the ip in PiIRPosSend.py and ForwardWandPosFromPiToUnity.py to match your IP found in previous step.
Make sure your firewall will accept communications from that specific IP and Port.