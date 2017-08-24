#!/usr/bin/env python
# -*- coding: UTF-8 -*-

import requests
import sys
from glob2 import glob
from os.path import dirname
from os.path import join
from os import environ


class Soe(object):
    """uploads Soe's"""

    def __init__(self, name, configuration):
        possible_soes = glob(join(dirname(__file__), '..', '..', '**', configuration, '{}.soe'.format(name)))
        file_name = ''

        if len(possible_soes) == 1:
            file_name = possible_soes[0]
        else:
            raise Exception('could not find the {} soe.'.format(name))

        host = environ.get('AGS_HOST')
        print('uploading {}'.format(file_name))
        print('to {}'.format(host))

        token_url = 'http://{}:6080/arcgis/admin/generateToken'.format(host)
        update_soe_url = 'http://{}:6080/arcgis/admin/services/types/extensions/update'.format(host)
        upload_url = 'http://{}:6080/arcgis/admin/uploads/upload?token={}'.format(host, '{}')

        data = {'username': environ.get('AGS_USER'), 'password': environ.get('AGS_PW'), 'client': 'requestip', 'f': 'json'}

        r = requests.post(token_url, data=data)
        data = {'f': 'json'}

        print('got token')

        files = {'itemFile': open(file_name, 'rb'), 'f': 'json'}

        data['token'] = r.json()['token']

        print('uploading')
        r = requests.post(upload_url.format(data['token']), files=files)

        print(r.status_code, r.json()['status'])

        data['id'] = r.json()['item']['itemID']

        print('updating')
        r = requests.post(update_soe_url, params=data)

        print(r.status_code, r.json()['status'])
        print('done')


if __name__ == '__main__':
    '''
    Usage:
        python soe.py <soe name> <configuration>  Publishes <soe> as <configuration>
    Arguments:
        <soe>             The name of the soe omitting the extension
        <configuration>   The visual studio folder name to find the .soe file in
    '''
    soe_name = sys.argv[1]
    build_configuration = sys.argv[2]

    soe = Soe(soe_name, build_configuration)
