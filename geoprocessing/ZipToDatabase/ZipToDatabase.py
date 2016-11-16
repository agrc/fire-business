#!/usr/bin/env python
# -*- coding: utf-8 -*-
'''
ZipToDatabase
----------------------------------
Given a .zip file with a shapefile, validate it, and then returns the feature as a graphic
'''
from Database import store_geometry_for
from os.path import join
from zipfile import ZipFile
import arcpy


required_files = ['shp', 'dbf', 'prj', 'shx']
utm = arcpy.SpatialReference(26912)


def extract_then_load(pk, zfilepath, test_method):
    #: open zip file and get paths
    zfile = ZipFile(zfilepath)
    zfilenames = zfile.namelist()
    zfile_exts = [name.split('.')[1] for name in zfilenames]
    zfile_name = zfilenames[0].split('.')[0]
    zfile_folder = join(arcpy.env.scratchFolder, zfile_name)
    shapefile = join(zfile_folder, zfile_name + '.shp')

    #: verify that all files are present
    for ext in required_files:
        if ext not in zfile_exts:
            raise Exception('Missing .{} file'.format(ext))

    zfile.extractall(zfile_folder)

    #: validate geometry
    checkgeom_output = 'in_memory/checkgeometry'
    arcpy.CheckGeometry_management(shapefile, checkgeom_output)

    if int(arcpy.GetCount_management(checkgeom_output).getOutput(0)) > 0:
        with arcpy.da.SearchCursor(checkgeom_output, ['PROBLEM']) as scur:
            raise Exception('Geometry Error: {}'.format(scur.next()[0]))

    #: validate geometry type for category
    described = arcpy.Describe(shapefile)

    if described.shapeType != 'Polygon':
        raise Exception('Incorrect shape type of {}. Fire perimeters are polygons.'.format(described.shapeType))

    messages = []

    #: reproject if necessary
    reprojected_fc = None
    input_sr = described.spatialReference
    if input_sr.name != utm.name:
        #: Project doesn't support the in_memory workspace
        messages.append('Reprojected data from {} to {}'.format(input_sr.factoryCode, utm.factoryCode))
        reprojected_fc = '{}/project'.format(arcpy.env.scratchGDB)
        shapefile = arcpy.Project_management(shapefile, reprojected_fc, utm)

    #: union all shapes in shapefile
    mergedGeometry = None
    features = 0
    with arcpy.da.SearchCursor(shapefile, ['SHAPE@']) as scur:
        for shape, in scur:
            features = features + 1
            if mergedGeometry is None:
                mergedGeometry = shape
                continue

            mergedGeometry = mergedGeometry.union(shape)

    if features == 0:
        raise Exception('Shapefile is empty')

    if features > 1:
        messages.append('Unioned {} features into one.'.format(features))

    #: delete temp data
    if reprojected_fc is not None and arcpy.Exists(reprojected_fc):
        arcpy.Delete_management(reprojected_fc)

    if arcpy.Exists(zfile_folder):
        arcpy.Delete_management(zfile_folder)

    #: insert geometry into database
    db_method = store_geometry_for
    if test_method is not None:
        db_method = test_method

    status, message = db_method(pk, mergedGeometry.WKT)

    if message is not None:
        messages.append(message)

    return (status, messages)


if __name__ == '__main__':
    result = extract_then_load(arcpy.GetParameterAsText(0), arcpy.GetParameterAsText(1))
    arcpy.SetParameterAsText(2, result['success'])
    arcpy.SetParameterAsText(3, result['message'])
