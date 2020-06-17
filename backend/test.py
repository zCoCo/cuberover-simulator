import types;
import sys;
import getopt;
import os;
import time;
from threading import Thread;

import csv;

### SETTINGS (defaults): ###
MISSION = 'paper'
PASS = ''
COMMANDS_HISTORY = "../Assets/Resources/commands.csv" # location of csv file containing log of all sent commands
TELEMETRY_HISTORY = "../Assets/Resources/telemetry.csv" # location of csv file containing log of all returned telemetry
IMAGES_DIR = "../"  # location of images directory

count = 1;
close = False;

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

            with open('test.csv', mode='a') as file:
                writer = csv.writer(file, delimiter=',', quotechar='"')
                writer.writerow([count])

            print(count);
            count = count + 1;
            if count > 10:
                close = True;

def main():
    # Print locations:
    print("Mission: ", MISSION, "@", PASS)
    print("Command Location: ", COMMANDS_HISTORY)
    print("Telemetry Location: ", TELEMETRY_HISTORY)
    print("Image Store: ", IMAGES_DIR)

    with open('test.csv', mode='a') as file:
        writer = csv.writer(file, delimiter=',', quotechar='"')
        writer.writerow([MISSION])
        writer.writerow([PASS])
        writer.writerow([COMMANDS_HISTORY])
        writer.writerow([TELEMETRY_HISTORY])
        writer.writerow([IMAGES_DIR])

    print(555);
    PrintInThreadDelay();

    while not close:
        pass  # Keep line open for threads (until close)

if __name__ == "__main__":
    # Load Arguments:
    loadArgs(sys.argv[1:])
    main()
