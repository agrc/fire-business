# Fire Business System Integration

## Usage

#### Test Environment
(_This is using data from a fake fire feature class. It will be more helpful when it is hooked up to a real database._)

- [Image generation and shapefile zip upload](http://test.mapserv.utah.gov/arcgis/rest/services/FireBusinessSystem/FireToolbox/GPServer)
- [Extract Intersection service](http://test.mapserv.utah.gov/arcgis/rest/services/FireBusinessSystem/FireAreas/MapServer/exts/FireBusinessSoe/ExtractIntersections)
- [Example requests and responses](https://gist.github.com/steveoh/42c89e58e1c98c8f7f9d66a4c4dc47d6)

## Installation

#### Server Object Extension

1. Run a `Release` build
1. set **User Environment Variables**
  - `AGS_HOST`: `localhost` or whatever server ip
  - `AGS_USER`: an arcgis server admin user
  - `AGS_PW`: the admin user's password
1. execute `python soe.py fire-business-soe Release` to upload and update the soe
  - if this is the first publish of the extension, you have to manually upload it and enable it on a map service

#### Geoprocessing Tools

**Before running Toolbox**

You may need to update the script path to the items in the toolbox to match the path on your machine.

#### Publishing

1. Run `ZipToDatabase` in ArcMap using `1` and `scripts/ZipToDatabase/tests/data/Poly_WGS.zip` in preparation for publishing.
1. Run `FireImage` in ArcMap using `1`, `400`, `800` in preparation for publishing.
1. Right click on a result and select share as service
1. Publish the service as: `FireBusiness/FireToolbox`
1. Add the result of the other GP 
1. Asynchronous
1. Allow Uploads
1. Message Level should be at least `Warning`. This is required to allow error message to be properly displayed from within the app.

**Make sure the mxd and all py files were published**

1. The operationaldata.gdb is often missing
1. The mxd needs repair
1. Database.py is not shipped
