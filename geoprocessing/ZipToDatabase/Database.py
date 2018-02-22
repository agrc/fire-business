#!/usr/bin/env python
# * coding: utf8 *
'''
database.py

A module that contains the methods to interact with a database
'''

import pyodbc
from connection import db

sql = 'UPDATE [Incident] SET [perimeter] = geometry::STGeomFromText(?, 26912) WHERE id = ?'
db_info = db.info


def store_geometry_for(pk, wkt):
    try:
        connection_string = db.get_connection_string(db_info)

        cnxn = pyodbc.connect(connection_string)
        cursor = cnxn.cursor()
    except Exception:
        return (False, 'There was a problem connecting to the database.')

    cursor.execute(sql, wkt, pk)

    if cursor.rowcount != 1:
        cnxn.rollback()

        return (False, 'There were {} affected rows when there should be 1.'.format(cursor.rowcount))

    cnxn.commit()

    return (True, '')
