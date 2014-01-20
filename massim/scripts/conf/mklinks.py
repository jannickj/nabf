#!/usr/bin/env python3

import os

path_conf = os.getenv('MAPC')+'/massim/scripts/conf/'
path_help = path_conf + 'helpers/2013/'

files_conf = [f for f in os.listdir() if os.path.isfile(f) and not '.py' in f]
files_help = [f for f in os.listdir('./helpers') if os.path.isfile('./helpers/'+f)]

ignore = ['2013-3sims.xml', 'helpers', 'visualization']

for f in os.listdir(path_conf):
    if f not in ignore:
        os.system('rm {}'.format(path_conf + f))

for f in files_conf:
    os.system('cp {} {}'.format(f, path_conf + f))

for f in files_help:
    if f in os.listdir(path_help):
        os.system('rm {}'.format(path_help + f))
    os.system('cp {} {}'.format('helpers/' + f, path_help + f))

