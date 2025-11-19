#!/bin/bash
export DISPLAY=:0
export XAUTHORITY=/home/boresight/.Xauthority

/usr/bin/lxterminal -e bash -lc '/usr/bin/python3 /home/boresight/ORCA-Simulator/RPI_Bridge/src/main.py; exec bash'
