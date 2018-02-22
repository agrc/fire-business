#!/usr/bin/env python
# -*- coding: utf-8 -*-
'''
GenerateImage.py
Given a project id, width, and height this service returns a png map image
showing the features for that project.
'''

from os.path import dirname, join

import arcpy

path_to_mxd = join(dirname(__file__), 'FireAreas.mxd')
image_path = join(arcpy.env.scratchFolder, 'map_export.png')


def main(project_id, width, height):
    mxd = arcpy.mapping.MapDocument(path_to_mxd)

    data_frame = arcpy.mapping.ListDataFrames(mxd)[0]
    layers = arcpy.mapping.ListLayers(mxd)

    feature_layers = [layers[0], layers[1]]
    where_clause = 'id={}'.format(project_id)
    utm = arcpy.SpatialReference(26912)

    bag = xmin = ymin = xmax = ymax = None
    offset = 10

    for l in feature_layers:
        l.definitionQuery = where_clause

        with arcpy.da.SearchCursor(l, 'SHAPE@', where_clause=where_clause) as cursor:
            for shape, in cursor:
                if shape is None:
                    continue

                if shape.spatialReference.factoryCode != 26912:
                    shape = shape.projectAs(utm, 'NAD_1983_To_WGS_1984_5')

                if shape.type == 'point':
                    point = shape.centroid

                    if point is None or point.X == 0:
                        arcpy.AddMessage('Empty geometry found in {} id: {}'.format(l.name, project_id))
                        continue

                    shape = arcpy.Polygon(arcpy.Array([arcpy.Point(point.X - offset, point.Y - offset),
                                                       arcpy.Point(point.X + offset, point.Y + offset),
                                                       arcpy.Point(point.X - offset, point.Y + offset),
                                                       arcpy.Point(point.X + offset, point.Y - offset)]),
                                          utm)

                if bag is None:
                    bag = shape
                    continue

                bag = bag.union(shape)

    #: validate that features were found
    if bag is None:
        arcpy.AddMessage('No features found for project id: {}'.format(project_id))

        return ''

    extent = bag.extent

    xmax = extent.XMax
    xmin = extent.XMin
    ymax = extent.YMax
    ymin = extent.YMin

    if xmax == xmin:
        xmax = extent.XMax + offset
        xmin = extent.XMin - offset
        ymax = extent.YMax + offset
        ymin = extent.YMin - offset

    delta_x = (xmax - xmin) * .25
    delta_y = (ymax - ymin) * .25

    data_frame.extent = arcpy.Extent(xmin - delta_x, ymin - delta_y, xmax + delta_x, ymax + delta_y)

    arcpy.mapping.ExportToPNG(mxd, image_path, data_frame=mxd.activeDataFrame, df_export_width=int(width), df_export_height=int(height))

    print(image_path)
    return image_path


if __name__ == '__main__':
    result = main(arcpy.GetParameterAsText(0), arcpy.GetParameterAsText(1), arcpy.GetParameterAsText(2))
    arcpy.SetParameterAsText(3, result)
