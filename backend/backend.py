# Watches for new commands in the DB which it puts in the log CSV file, which is
# then read by the simulation which saves data to images in the images directory.
# When new images are detected in the directory, this uploads them into the
# database:
#
# Call | Options List:
# -m    Mission Name
# -p    Mission Pass
# -c    Commands CSV File Address
# -t    Telemetry CSV File Address
# -i    Images Directory Location

import types;
import sys;
import getopt;
import os;
import time;
from datetime import datetime;
from threading import Thread;

import pprint;

from mongo_db_collection import MongoDbCollection
import csv;

from PIL import Image, ImageOps
import numpy as np

### SETTINGS (defaults): ###
MISSION = 'paper'
PASS = ''
COMMANDS_HISTORY = "../Assets/Resources/commands.csv" # location of csv file containing log of all sent commands
TELEMETRY_HISTORY = "../Assets/Resources/telemetry.csv" # location of csv file containing log of all returned telemetry
IMAGES_DIR = "../"  # location of images directory

ENABLE_LOGGING = True
LOG_FILE = 'Log.csv'


DB = types.SimpleNamespace()

pp = pprint.PrettyPrinter(indent=2)

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

def nameToData(name):
    """
    Extracts ImageData from the Given Filename. Filename must take the form:
    CommandLookupID-NAME-CAMERA-lookupID-unixTimestamp
    """
    data = dict();
    features = name.split('-');
    if len(features) and len(features) >= 5:
        [commandLookupID,name,camera,lookupID,sendTime,*extra] = features;
        data["name"] = name;
        data["camera"] = camera;
        data["lookupID"] = int(lookupID); # Gets overridden with count+1 when written to DB
        data["commandLookupID"] = int(commandLookupID);
        data["sendTime"] = int(sendTime);
    else:
        data = None

    return data;




def upload_data(f, addr):
    file = DB.images.fs.put(f);
    data = nameToData(os.path.splitext(addr)[0]);
    print("Attempting upload of %s" % addr)
    if data is not None:
        data["file"] = file;  # file location in GridFS
        DB.commands.update(data['commandLookupID'], {'stateFp': 'SUCC_EXEC'}) # Do this first so it returns first
        DB.images.write(data);
        print("#### Uploaded %s ####" % addr)
        Log(["#### Uploaded %s ####" % addr])
        pp.pprint(data)

def process_image(addr):
    # Check if science image:
    is_science = "science" in addr.lower()

    # Load and convert:
    img = Image.open(addr)
    gray = ImageOps.grayscale(img)
    w,h = gray.size

    # Resize:
    target_dims = np.asarray([2594, 1944]) if is_science else np.asarray([2594, 1944])/4
    required_scaling = target_dims / np.asarray([w,h])
    scale = np.max(required_scaling)
    scaled = gray.resize((int(scale*w), int(scale*h)))
    scaled_w, scaled_h = scaled.size

    # Crop:
    left = int((scaled_w-target_dims[0])/2)
    top = int((scaled_h-target_dims[1])/2)
    right = int(left + target_dims[0])
    bottom = int(top + target_dims[1])
    cropped = scaled.crop((left,top,right,bottom))

    # Save:
    path, ext = os.path.splitext(addr)
    out_addr = path + "-missionmodified" + ext
    quality = 100 if is_science else 20
    cropped.save(out_addr, 'JPEG', dpi=[300,300], quality=quality)
    return out_addr

def upload_files(files):
    for address in files:
        if address.endswith(('.jpeg', '.jpg')) and not address.startswith('.') and "-missionmodified" not in address:
            uri = os.path.join(IMAGES_DIR, address)
            out_uri = process_image(uri)
            with open(out_uri, "rb") as im_file:
                upload_data(im_file, address)

# Upload all existing images:
#upload_files(os.listdir(IMAGES_DIR))

# Watch for changes:
class UploadNewImages(Thread):
    def __init__(self):
        Thread.__init__(self)
        self.daemon = True
        self.start()

    def run(self):
        before = dict([(f, None) for f in os.listdir(IMAGES_DIR)])
        while 1:
            time.sleep(1)
            after = dict([(f, None) for f in os.listdir(IMAGES_DIR)])
            added = [f for f in after if not f in before]
            if added:
                print("NEW IMAGE DETECTED.")
                Log(["NEW IMAGE DETECTED."])
                upload_files(added)
            before = after


class WatchCommands(Thread):
    def __init__(self):
        Thread.__init__(self)
        self.daemon = True
        self.start()

    def run(self):
        # Monitor the change stream:
        for c in DB.commands.changes:
            # The change stream (and, thereby, this iterator) only closes when an error
            # occurs, the collection or database is tossed, or sometimes when a shard is
            # removed.
            #
            # So, by leaving this loop open (say, in its own thread) anytime there's a
            # change to the commands collection, the following line gets run for all
            # changed commands `c`:

            # Randomly update changed documents as being successfully sent to the rover or failed:
            DB.commands.update(c['lookupID'], {'stateFp': 'SUCC_SENT'})
            print("### NEW COMMAND ###")
            pp.pprint(c)
            with open(COMMANDS_HISTORY, mode='a') as file:
                writer = csv.writer(file, delimiter=',', quotechar='"')
                if c['name'] == 'MoveForward' or c['name'] == 'MoveBackward':
                    turn = 0
                    amount = c['args']['Distance']
                    speed = c['args']['Speed']
                    if c['name'] == 'MoveForward':
                        dir = 1;
                    else:
                        dir = -1;
                elif c['name'] == 'TurnLeft' or c['name'] == 'TurnRight':
                    turn = 1
                    amount = c['args']['Angle']
                    speed = c['args']['Speed']
                    if c['name'] == 'TurnLeft':
                        dir = -1;
                    else:
                        dir = 1;
                elif c['name'] == 'ARTEMIS_SetSignalDelay':
                    amount = c['args']['Delay']
                    speed = 0
                    turn = 0
                    dir = 0
                else:
                    amount = 0
                    speed = 0
                    turn = 0
                    dir = 0

                writer.writerow([c['lookupID'], c['name'], amount, speed, turn, dir])
                Log(["New Command: ", c['lookupID'], c['name'], amount, speed, turn, dir])


def main():
    # Print locations:
    print("Mission: ", MISSION)
    print("Command Location: ", COMMANDS_HISTORY)
    print("Telemetry Location: ", TELEMETRY_HISTORY)
    print("Image Store: ", IMAGES_DIR)
    Log([MISSION,PASS,COMMANDS_HISTORY,TELEMETRY_HISTORY,IMAGES_DIR])

    # Connect to the Images Collection:
    DB.images = MongoDbCollection(partition=MISSION, code=PASS, collection=MongoDbCollection.Names.IMAGES);
    # Connect to the Commands Collection:
    DB.commands = MongoDbCollection(partition=MISSION, code=PASS, collection=MongoDbCollection.Names.COMMANDS);

    # Start Threading:
    UploadNewImages();
    WatchCommands();
    while 1:
        pass  # Keep line open for threads


if __name__ == "__main__":
    # Load Arguments:
    loadArgs(sys.argv[1:])
    main()
