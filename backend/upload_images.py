# Uploads all the images from the given directory into the database:
import sys;
import os;
import time;
import base64;

from mongo_db_collection import MongoDbCollection
DB = MongoDbCollection(partition='Paper9001', code='RedRover', collection=MongoDbCollection.Names.IMAGES);

def nameToData(name):
    """
    Extracts ImageData from the Given Filename. Filename must take the form:
    NAME-CAMERA-lookupID-CommandLookupID-unixTimestamp
    """
    data = dict();
    features = name.split('-');
    if len(features):
        [name,camera,lookupID,commandLookupID,sendTime] = features;
        data["name"] = name;
        data["camera"] = camera;
        data["lookupID"] = int(lookupID);
        data["commandLookupID"] = int(commandLookupID);
        data["sendTime"] = int(sendTime);

    return data;


targDir = os.path.join("./", sys.argv[1]);

# Upload all existing images:
for address in os.listdir(targDir):
    if address.endswith(('.jpeg')) and not address.startswith('.'):
        with open(os.path.join(targDir, address), "rb") as im_file:
            file = DB.fs.put(im_file);
            data = nameToData(os.path.splitext(address)[0]);
            data["file"] = file; # file location in GridFS
            DB.write(data);


