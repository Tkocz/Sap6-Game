#!/usr/bin/env python3

#---------------------------------------
# IMPORTS
#---------------------------------------

import argparse
import getpass
import os
import sys
import time

#---------------------------------------
# CONSTANTS
#---------------------------------------

AUTHOR  = "Philip Arvidsson <philip@philiparvidsson.com>"
VERSION = "0.1b"

ERR_NO_LOG_EXISTS = 3

LOG_DIR = "../Meta/Work-Logs"

#---------------------------------------
# FUNCTIONS
#---------------------------------------

def confirm(question, default="yes"):
    valid = { "no": False, "n": False, "yes": True, "y": True }

    if default is None    : prompt = " [y/n] "
    elif default == "yes" : prompt = " [Y/n] "
    elif default == "no"  : prompt = " [y/N] "

    while True:
        sys.stdout.write(question + prompt)
        choice = input().lower()
        if   default is not None and choice == '' : return valid[default]
        elif choice in valid                      : return valid[choice]

def parse_args():
    ap = argparse.ArgumentParser()

    ap.add_argument("-d", "--description", nargs="+", type=str,
        help="specify work description")

    ap.add_argument("-u", "--user", default=None, type=str,
        help="specify user name")

    ap.add_argument("-s", "--stats", action="store_true",
        help="display work log statistics")

    ap.add_argument("-t", "--time", type=float,
        help="specify work in number of hours")

    args = ap.parse_args()

    if args.user is None:
        args.user = getpass.getuser().capitalize()

    return args

#---------------------------------------
# ENTRY POINT
#---------------------------------------

if __name__ == "__main__":
    args = parse_args()

    # Make sure the log directory exists.
    if not os.path.exists(LOG_DIR):
        os.mkdir(LOG_DIR)

    filename = os.path.join(LOG_DIR, args.user + ".wl")

    if not os.path.exists(filename):
        r = confirm(f"Create new work log for user {args.user}?")
        if not r:
            sys.exit(ERR_NO_LOG_EXISTS)

    description = " ".join(args.description)
    s = f"{int(time.time())}:{args.time}:{description}\n"
    with open(filename, "a") as f:
        f.write(s)

    print("Log entry written.")
