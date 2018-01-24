#!/usr/bin/env python
# -*- coding: utf-8 -*-
'''
GenerateImage.py
Given a project id, width, and height this service returns a png map image
showing the features for that project.
'''

from arcpy import env
from arcpy import Extent
from arcpy import GetParameterAsText
from arcpy import mapping
from arcpy import SetParameterAsText
from math import isnan
from os.path import dirname
from os.path import join

path_to_mxd = join(dirname(__file__), 'fire.mxd')
image_path = join(env.scratchFolder, 'map_export.png')


def main(project_id, width, height):
    mxd = mapping.MapDocument(path_to_mxd)
    data_frame = mapping.ListDataFrames(mxd)[0]
    feature_layers = [mapping.ListLayers(mxd)[0]]
    xmin = None
    ymin = None
    xmax = None
    ymax = None

    for l in feature_layers:
        l.definitionQuery = 'id = {}'.format(project_id)
        extent = l.getExtent()
        if xmin is None or isnan(xmin) or extent.XMin < xmin:
            xmin = extent.XMin
        if ymin is None or isnan(ymin) or extent.YMin < ymin:
            ymin = extent.YMin
        if xmax is None or isnan(xmax) or extent.XMax > xmax:
            xmax = extent.XMax
        if ymax is None or isnan(ymax) or extent.YMax > ymax:
            ymax = extent.YMax

    #: validate that features were found
    if isnan(xmin):
        raise Exception('No features found for project id: {}!'.format(project_id))

    data_frame.extent = Extent(xmin, ymin, xmax, ymax)

    mapping.ExportToPNG(mxd, image_path, data_frame=mxd.activeDataFrame, df_export_width=int(width), df_export_height=int(height))

    print(image_path)
    return image_path


if __name__ == '__main__':
    result = main(GetParameterAsText(0), GetParameterAsText(1), GetParameterAsText(2))
    SetParameterAsText(3, result)
