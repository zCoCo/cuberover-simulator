# Basic (non-DB related) Functionality Tests for backend.py
import types;
import sys;
import getopt;
import os;
import time;
from datetime import datetime;
from threading import Thread;

import zmq

import csv;

### SETTINGS (defaults): ###
MISSION = 'paper'
PASS = ''
COMMANDS_HISTORY = "../Assets/Resources/commands.csv" # location of csv file containing log of all sent commands
TELEMETRY_HISTORY = "../Assets/Resources/telemetry.csv" # location of csv file containing log of all returned telemetry
IMAGES_DIR = "../"  # location of images directory

count = 1;
close = False;

ENABLE_LOGGING = True
LOG_FILE = 'test.csv'
# Adds the given entries to the log
def Log(entries):
    if ENABLE_LOGGING:
        with open(LOG_FILE, mode='a') as file:
            writer = csv.writer(file, delimiter=',', quotechar='"')
            writer.writerow([datetime.now().strftime("%m/%d/%Y @ %H:%M:%S.%f")[:-3] + ": "] + entries)

def loadArgs(argv):
    global MISSION, PASS, COMMANDS_HISTORY, TELEMETRY_HISTORY, IMAGES_DIR

    helpstr = 'usage: backend.py -m <mission_name> -p <mission_pass> -c <commands_csv> -t <telem_csv> -i <image_dir>'
    try:
        opts, args = getopt.getopt(argv, "hm:p:c:t:i:", ["help", "mission_name=", "mission_pass=", "commands_csv=", "telem_csv=", "image_dir="])
    except getopt.GetoptError:
        print('backend.py: INVALID ARGS.')
        print(helpstr)
        sys.exit(2)

    for opt, arg in opts:
        if opt in ('-h', '--help'):
            print(helpstr)
            sys.exit()
        elif opt in('-m', '--mission_name'):
            MISSION = arg
        elif opt in ('-p', '--mission_pass'):
            PASS = arg
        elif opt in ('-c', '--commands_csv'):
            COMMANDS_HISTORY = arg
        elif opt in ('-t', '--telem_csv'):
            TELEMETRY_HISTORY = arg
        elif opt in ('-i', '--image_dir'):
            IMAGES_DIR = arg

class PrintInThreadDelay(Thread):
    def __init__(self):
        Thread.__init__(self)
        self.daemon = True
        self.start()

    def run(self):
        global count, close;
        while 1:
            time.sleep(1);

            Log([count]);

            print(count);
            count = count + 1;
            if count > 10:
                close = True;

class ZMQServer(Thread):
    def __init__(self):
        self.context = zmq.Context()
        self.socket = self.context.socket(zmq.REP)
        self.socket.bind("tcp://*:5555")

        Thread.__init__(self)
        self.daemon = True
        self.start()

    def run(self):
        Log(["Socket Connected."]);
        while 1:
            #  Wait for next request from client
            message = self.socket.recv()
            print("Received request: %s" % message)

            # Do "work"
            time.sleep(0.25)

            #  Send reply back to client
            self.socket.send(b"World")

def main():
    # Print locations:
    print("Mission: ", MISSION, "@", PASS)
    print("Command Location: ", COMMANDS_HISTORY)
    print("Telemetry Location: ", TELEMETRY_HISTORY)
    print("Image Store: ", IMAGES_DIR)

    Log([MISSION,PASS,COMMANDS_HISTORY,TELEMETRY_HISTORY,IMAGES_DIR])

    PrintInThreadDelay();
    ZMQServer();

    while not close:
        pass  # Keep line open for threads (until close)

if __name__ == "__main__":
    # Load Arguments:
    loadArgs(sys.argv[1:])
    main()
