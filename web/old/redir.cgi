#!/bin/python

import sys
sys.path.insert(0, "/usr/home/joe/lib/python")
sys.path.insert(0, "/usr/local/lib/python")

import os
import cgi
import cgitb
from time import asctime

cgitb.enable()

form = cgi.FieldStorage()
if form.has_key("w"):
	f = open("download.txt","a")
	f.write(asctime()+":"+os.environ["REMOTE_ADDR"]+":"+os.environ.get("HTTP_USER_AGENT","")+":"+form.getfirst("w")+"\n")
	f.close()
	print "Location: "+form.getfirst("w")
	print
else:
	print "Content-Type: text/plain"
	print
