# Uploads all the images from the given directory into the database:
import sys;
import os;
import time;
from threading import Thread;

import pprint;

from mongo_db_collection import MongoDbCollection
import csv;

### SETTINGS: ###
MISSION = 'paper2006042'
targDir = "../"  # relative location of images directory
COMMANDS_HISTORY = "../Assets/Resources/commands.csv" # relative location of csv file containing log of all sent commands


# Connect to the Images Collection:
DB = MongoDbCollection(partition=MISSION, code='RedRover', collection=MongoDbCollection.Names.IMAGES);
# Connect to the Commands Collection:
commands = MongoDbCollection(partition=MISSION, code='RedRover', collection=MongoDbCollection.Names.COMMANDS);

pp = pprint.PrettyPrinter(indent=2)


def nameToData(name):
    """
    Extracts ImageData from the Given Filename. Filename must take the form:
    CommandLookupID-NAME-CAMERA-lookupID-unixTimestamp
    """
    data = dict();
    features = name.split('-');
    if len(features) and len(features) == 5:
        [commandLookupID,name,camera,lookupID,sendTime] = features;
        data["name"] = name;
        data["camera"] = camera;
        data["lookupID"] = int(lookupID); # Gets overridden with count+1 when written to DB
        data["commandLookupID"] = int(commandLookupID);
        data["sendTime"] = int(sendTime);
    else:
        data = None

    return data;




def upload_data(f, addr):
    file = DB.fs.put(f);
    data = nameToData(os.path.splitext(addr)[0]);
    print("Attempting upload of %s" % addr)
    if data is not None:
        data["file"] = file;  # file location in GridFS
        commands.update(data['commandLookupID'], {'stateFp': 'SUCC_EXEC'}) # Do this first so it returns first
        DB.write(data);
        print("#### Uploaded %s ####" % addr)
        pp.pprint(data)


def upload_files(files):
    for address in files:
        if address.endswith(('.jpeg', '.jpg')) and not address.startswith('.'):
            with open(os.path.join(targDir, address), "rb") as im_file:
                upload_data(im_file, address)


# Upload all existing images:
#upload_files(os.listdir(targDir))

# Watch for changes:
class UploadNewImages(Thread):
    def __init__(self):
        Thread.__init__(self)
        self.daemon = True
        self.start()

    def run(self):
        before = dict([(f, None) for f in os.listdir(targDir)])
        while 1:
            time.sleep(5)
            after = dict([(f, None) for f in os.listdir(targDir)])
            added = [f for f in after if not f in before]
            if added:
                print("NEW IMAGE DETECTED.")
                upload_files(added)
            before = after


class WatchCommands(Thread):
    def __init__(self):
        Thread.__init__(self)
        self.daemon = True
        self.start()

    def run(self):
        # Monitor the change stream:
        for c in commands.changes:
            # The change stream (and, thereby, this iterator) only closes when an error
            # occurs, the collection or database is tossed, or sometimes when a shard is
            # removed.
            #
            # So, by leaving this loop open (say, in its own thread) anytime there's a
            # change to the commands collection, the following line gets run for all
            # changed commands `c`:

            # Randomly update changed documents as being successfully sent to the rover or failed:
            commands.update(c['lookupID'], {'stateFp': 'SUCC_SENT'})
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


UploadNewImages();
WatchCommands();
while 1:
    pass  # Keep line open for threads
