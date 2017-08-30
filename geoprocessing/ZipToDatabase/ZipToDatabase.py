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


def extract_then_load(pk, zfilepath, test_method=None):
    #: open zip file and get paths
    arcpy.AddMessage('uploaded {}.'.format(zfilepath))
    zfile = ZipFile(zfilepath)
    zfilenames = zfile.namelist()
    zfile_exts = [name.split('.')[1] for name in zfilenames]
    zfile_name = zfilenames[0].split('.')[0]
    zfile_folder = join(arcpy.env.scratchFolder, zfile_name)
    shapefile = join(zfile_folder, zfile_name + '.shp')

    arcpy.AddMessage('verify that all files are present')
    #: verify that all files are present
    for ext in required_files:
        if ext not in zfile_exts:
            raise Exception('Missing .{} file'.format(ext))

    zfile.extractall(zfile_folder)

    arcpy.AddMessage('validating geometry')
    #: validate geometry
    checkgeom_output = 'in_memory/checkgeometry'
    arcpy.CheckGeometry_management(shapefile, checkgeom_output)

    if int(arcpy.GetCount_management(checkgeom_output).getOutput(0)) > 0:
        with arcpy.da.SearchCursor(checkgeom_output, ['PROBLEM']) as scur:
            arcpy.AddError('Geometry Error: {}'.format(scur.next()[0]))
            raise Exception('Geometry Error: {}'.format(scur.next()[0]))

    arcpy.AddMessage('validating geometry type')
    #: validate geometry type for category
    described = arcpy.Describe(shapefile)

    if described.shapeType != 'Polygon':
        arcpy.AddError('Incorrect shape type of {}. Fire perimeters are polygons.'.format(described.shapeType))
        raise Exception('Incorrect shape type of {}. Fire perimeters are polygons.'.format(described.shapeType))

    messages = []

    arcpy.AddMessage('reprojecting if necessary')
    #: reproject if necessary
    reprojected_fc = None
    input_sr = described.spatialReference
    if input_sr.name != utm.name:
        #: Project doesn't support the in_memory workspace
        arcpy.AddMessage('Reprojected data from {} to {}'.format(input_sr.factoryCode, utm.factoryCode))
        messages.append('Reprojected data from {} to {}'.format(input_sr.factoryCode, utm.factoryCode))
        reprojected_fc = '{}/project'.format(arcpy.env.scratchGDB)
        shapefile = arcpy.Project_management(shapefile, reprojected_fc, utm)

    arcpy.AddMessage('unioning all shapes')
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
        arcpy.AddError('Shapefile is empty')
        raise Exception('Shapefile is empty')

    if features > 1:
        arcpy.AddMessage('Unioned {} features into one.'.format(features))
        messages.append('Unioned {} features into one.'.format(features))

    arcpy.AddMessage('cleaning up temp data')
    #: delete temp data
    if reprojected_fc is not None and arcpy.Exists(reprojected_fc):
        arcpy.Delete_management(reprojected_fc)

    if arcpy.Exists(zfile_folder):
        arcpy.Delete_management(zfile_folder)

    arcpy.AddMessage('inserting geometry')
    #: insert geometry into database
    db_method = store_geometry_for
    if test_method is not None:
        db_method = test_method

    status, message = db_method(pk, mergedGeometry.WKT)
    arcpy.AddMessage('db response {}, {}'.format(status, message))

    if message is not None:
        messages.append(message)

    return (status, messages)


if __name__ == '__main__':
    status, messages = extract_then_load(arcpy.GetParameterAsText(0), arcpy.GetParameterAsText(1))
    arcpy.SetParameterAsText(2, status)
    arcpy.SetParameterAsText(3, messages)
