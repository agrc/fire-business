# Fire Business System Integration

## Server Object Extension



## Geoprocessing

#### Before running Toolbox

You may need to update the path to `ZipToDatabase.py` in the toolbox to match the path on your machine.

#### Publishing

1. Use `1` and scripts/ZipToDatabase/tests/data/Poly_WGS.zip` to run the tool in ArcMap in preparation for publishing.
1. Publish the service as: `FireBusiness/Toolbox`
1. Asynchronous
1. Allow Uploads
1. Message Level should be at least `Warning`. This is required to allow error message to be properly displayed from within the app.

#### Tests

1. `cd geoprocessing`
1. `nosetests -w ZipToDatabase`
