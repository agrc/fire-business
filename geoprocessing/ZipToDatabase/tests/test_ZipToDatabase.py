#!/usr/bin/env python
# -*- coding: utf-8 -*-

'''
test_ZipToDatabase
----------------------------------
test the ZipToDatabase module
'''

from mock import Mock
from nose.tools import raises
from os.path import join
from ZipToDatabase import ZipToDatabase
import arcpy
import unittest


class TestZipToGraphics(unittest.TestCase):

    primary_key = 1

    def tearDown(self):
        # clear up temp data
        names = ['checkgeometry']

        for n in names:
            arcpy.Delete_management('in_memory/' + n)

        arcpy.Delete_management('{}/project'.format(arcpy.env.scratchGDB))

    @raises(Exception)
    def test_checks_for_prj_file(self):
        path = join('tests', 'data', 'Missing_Prj.zip')
        ZipToDatabase.extract_then_load(self.primary_key, path)

    def test_utm(self):
        path = join('tests', 'data', 'UTM.zip')
        success, messages = ZipToDatabase.extract_then_load(self.primary_key, path, Mock(return_value=(True, None)))

        self.assertTrue(success)
        self.assertEqual(len(messages), 0)

    def test_multiple_features(self):
        path = join('tests', 'data', 'Multiple_Features.zip')
        success, messages = ZipToDatabase.extract_then_load(self.primary_key, path, Mock(return_value=(True, None)))

        self.assertTrue(success)
        self.assertEqual(len(messages), 2)
        self.assertEqual(messages[1], 'Unioned 2 features into one.')

    def test_reproject(self):
        path = join('tests', 'data', 'Poly_WGS.zip')
        success, messages = ZipToDatabase.extract_then_load(self.primary_key, path, Mock(return_value=(True, None)))

        self.assertTrue(success)
        self.assertEqual(len(messages), 1)
        self.assertEqual(messages[0], 'Reprojected data from 3857 to 26912')

    @raises(Exception)
    def test_multi_point(self):
        #: only polygons are acceptable
        path = join('tests', 'data', 'Multi_Point.zip')
        ZipToDatabase.extract_then_load(self.primary_key, path)

    @raises(Exception)
    def test_self_intersecting(self):
        path = join('tests', 'data', 'Self_Intersecting.zip')
        ZipToDatabase.extract_then_load(self.primary_key, path)
