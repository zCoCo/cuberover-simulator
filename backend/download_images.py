# Downloads all the images from the database into this directory:

def dataToName(data):
    """
    Converts all the relevant metadata from the given ImageData into an
    identifiable filename. Filename will take the form:
    NAME-CAMERA-lookupID-CommandLookupID-unixTimestamp
    """
    return data["name"]+"-"+data["camera"]+"-"+data["lookupID"]+"-"+data["commandLookupID"]+"-"+data["sendTime"];
