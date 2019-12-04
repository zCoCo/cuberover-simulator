#!/usr/bin/env python3
"""
    Demo of Main Functionality of the Collection Interface for the DB. This also
    can serve as a very poor set of unit tests.
    N.B. There need to be at least 12 commands in the collection for this demo
    to work properly.

    Author: Connor W. Colombo, CubeRover
    Last Updated: 6/19/2019
"""

import inspect
import os
import sys

import pprint
import random
import mongo_db_collection

pp = pprint.PrettyPrinter(indent=2)

### BASIC FUNCTIONALITY ###

# Connect to the Commands Collection:
commands = mongo_db_collection.MongoDbCollection(partition='Paper9001', code='RedRover')

print("### - 1 - ###")
# Get the command payload with lookupID 2:
c = commands.fetch(2)
###
pp.pprint(c)
assert isinstance(c, dict) # Returns a single document as a dictionary

print("### - 2 - ###")
# Use MongoDB query selector to find all command payloads which have a lookupID
# above 10 and below 20.
c = commands.fetch({'$gt': 10, '$lt': 20})
###
pp.pprint(c)
assert isinstance(c, list) # Returns a list of dicts, even if there is only one match to the query

print("### - 3 - ###")
# Update the state of the document with lookupID 2:
commands.update(2, {'stateFp': 'SUCC_SENT'})
###
c = commands.fetch(2, True)
pp.pprint(c)
assert c['stateFp'] == 'SUCC_SENT'

print("### - 4 - ###")
# Update state of all commands with lookupIDs above 10 and below 20:
commands.update({'$gt': 10, '$lt': 20}, {'stateFp': 'SUCC_SENT', 'errorFp': 'Woops. Someone ran the demo.'})
###
cs = commands.fetch({'$gt': 10, '$lt': 20}, True)
pp.pprint(cs)
assert(len(cs) == len([c for c in cs if c['stateFp'] == 'SUCC_SENT'])) # They're all now set to 'SUCC_SENT'

print("### - 5 - ###")
# Get the total number of commands by sizing up a list comprehension (and ensure iterator works):
l1 = len([c for c in commands])
# Get the total number of commands (and ensure aggregation counter works):
l2 = commands.count()
# Get the total number of commands from number of documents returned:
l3 = len(commands.fetch({'$gt': -1}))
###
pp.pprint(l1)
assert(l1 == l2)
assert(l1 == l3)

### ITERATORS AND CHANGE STREAMS ###

print("### - 6 - ###")
# Loop through all commands in the collection:
for c in commands:
    pp.pprint(c)
    # Update the state of commands to success:
    commands.update(c['lookupID'], {'stateFp': 'SUCC_SENT'})

print("### - 7: Change Stream: - ###")
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
    pp.pprint(c)
