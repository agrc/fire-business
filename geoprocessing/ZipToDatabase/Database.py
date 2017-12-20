#!/usr/bin/env python
# * coding: utf8 *
'''
database.py

A module that contains the methods to interact with a database
'''

import pyodbc

sql = 'UPDATE [Incident] SET [perimeter] = geometry::STGeomFromText(?, 26912) WHERE id = ?'


def store_geometry_for(pk, wkt):
    cnxn = pyodbc.connect('DRIVER={SQL Server};SERVER=(local);DATABASE=fire;Trusted_Connection=yes;')
    cursor = cnxn.cursor()

    cursor.execute(sql, wkt, pk)

    if cursor.rowcount != 1:
        cnxn.rollback()
        return (False, 'There were {} affected rows when there should be 1.'.format(cursor.rowcount))

    cnxn.commit()

    return (True, '')
