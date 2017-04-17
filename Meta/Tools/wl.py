#!/usr/bin/env python3.6

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

ERR_NO_SUCH_LOG = 3

LOG_DIR      = "../Work-Logs"
LOG_FILE_EXT = ".wl"

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

    ap.add_argument("-s", "--statistics", action="store_true",
        help="display work log statistics")

    ap.add_argument("-t", "--time", type=float,
        help="specify work time in number of hours")

    args = ap.parse_args()

    if args.description is None and not args.statistics:
        ap.print_help()
        sys.exit(2)

    if args.user is None:
        args.user = getpass.getuser()

    args.user = args.user.capitalize()

    return args

def show_statistics(args):
    filename = os.path.join(LOG_DIR, args.user + LOG_FILE_EXT)

    if not os.path.exists(filename):
        print(f"No such log file exists: {filename}")
        sys.exit(ERR_NO_SUCH_LOG)

    with open(filename, "r") as f:
        data = f.readlines()

    print(f"\nStatistics for {args.user}")

    def parse_line(s):
        a = s.split(":", 2)

        r = lambda: None

        r.timestamp = int(a[0])
        r.worktime  = float(a[1])
        r.desc      = a[2]

        return r

    parse_data = lambda fn: map(fn, map(parse_line, data))

    print(32*"-")

    total_hours = sum(parse_data(lambda d: d.worktime))

    print(f"  Total hours    : {total_hours:.2f}")

    start_timestamp = min(parse_data(lambda d: d.timestamp))
    end_timestamp   = max(parse_data(lambda d: d.timestamp))
    total_time      = (end_timestamp - start_timestamp) / 3600.0
    hours_per_week  = 168.0 * total_hours / total_time

    print(f"  Hours per week : {hours_per_week:.2f}")

def write_entry(args):
    filename = os.path.join(LOG_DIR, args.user + ".wl")

    if not os.path.exists(filename):
        r = confirm(f"Create new work log for user {args.user}?")
        if not r:
            print("User aborted.")
            sys.exit(ERR_NO_SUCH_LOG)

    description = " ".join(args.description)
    s = f"{int(time.time())}:{args.time}:{description}\n"
    with open(filename, "a") as f:
        f.write(s)

    print("Log entry written.")

#---------------------------------------
# ENTRY POINT
#---------------------------------------

if __name__ == "__main__":
    print(f"Work Log Tool v{VERSION}\n"
          f"  {AUTHOR}")

    args = parse_args()

    # Make sure the log directory exists.
    if not os.path.exists(LOG_DIR):
        os.mkdir(LOG_DIR)

    if args.statistics : show_statistics(args)
    else               : write_entry(args)
