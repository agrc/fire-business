# Fire Business System Integration

## Usage

#### Test Environment

- [Image generation and shapefile zip upload](http://maps.ffsl.utah.gov/arcgis/rest/services/Staging/FireToolbox/GPServer)
- [Extract Intersection service](http://maps.ffsl.utah.gov/arcgis/rest/services/Staging/FireAreas/MapServer/exts/FireBusinessSoe/ExtractIntersections)
- [Example requests and responses](https://gist.github.com/steveoh/42c89e58e1c98c8f7f9d66a4c4dc47d6)

## Installation

### Server Object Extension

1. Run a `Release` build
1. set **User Environment Variables**
  - `AGS_HOST`: `localhost` or whatever server ip
  - `AGS_USER`: an arcgis server admin user
  - `AGS_PW`: the admin user's password
1. execute `python soe.py fire-business-soe Release` to upload and update the soe
  - if this is the first publish of the extension, you have to manually upload it and enable it on a map service

### Geoprocessing Tools

**Before running Toolbox**

1. Update the script path to the items in the toolbox
1. Update/create [db.py](https://github.com/agrc/fire-business/blob/master/geoprocessing/ZipToDatabase/connection/secrets.db.py)

### Publishing

1. Run `ZipToDatabase` in ArcMap using `1` and `scripts/ZipToDatabase/tests/data/Poly_WGS.zip` in preparation for publishing.
1. Run `FireImage` in ArcMap using `1`, `400`, `800` in preparation for publishing.
1. Right click on a result and select share as service
1. Publish the service as: `FireBusiness/FireToolbox`
1. Add the result of the other GP 
1. Allow Uploads
1. Synchronous
1. Message Level should be at least `Warning`. This is required to allow error message to be properly displayed from within the app.
1. Pooling `0`-`3` `300`, `30`, `300`

**Make sure the mxd and all py files were published**

1. The operationaldata.gdb is often missing and the mxd needs repair
   - You can copy the gdb and mxd from the firearea's map service
1. `Database.py` and the `connection` package is not shipped
1. Verify all [requirements.txt](https://github.com/agrc/fire-business/blob/master/geoprocessing/requirements.txt) dependencies are installed
