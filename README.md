# MarkLogic ArcGIS Pro Addin

This project aims to provide Esri's [ArcGIS Pro](https://pro.arcgis.com) the capability to access and search a 
MarkLogic database straight from within the desktop application.  It is built using the .NET platform and the 
[ArcGIS Pro SDK](https://pro.arcgis.com/en/pro-app/sdk/).

## Dependencies

This solution has the following external dependencies:

- [ArcGIS Pro SDK for .NET](https://github.com/Esri/arcgis-pro-sdk/wiki/ProGuide-Installation-and-Upgrade) (at least version 2.1.0.10257)
- [Json.NET](https://www.newtonsoft.com/json) (at least version 10.0.3)
- .NET Framework 4.6.1
- The `marklogic-ui` server running on `http://esri-koop.demo.marklogic.com:8099`.

## Installation and Setup

Download and install [Visual Studio 2017](https://www.visualstudio.com/vs/community/) (Community or higher edition).
Ensure that at least the **.NET desktop development** workload is selected when choosing your installation configuration.

Install both [ArcGIS Pro SDK for .NET](https://github.com/Esri/arcgis-pro-sdk/wiki/ProGuide-Installation-and-Upgrade)
Visual Studio Extensions by following the instructions [here](https://github.com/Esri/arcgis-pro-sdk/wiki/ProGuide-Installation-and-Upgrade).

## Building the Project

### via Visual Studio
Open the `arcgis-ml-addin.sln` solution file in Visual Studio.

Under the `Build` menu, select `Rebuild Solution`.

> Note: The ArcGIS Pro SDK assembly references persist themselves as absolute paths in the .csproj project file.
> If you encounter unresolved assembly reference errors when building, this can be remedied by taking the following steps:
> - In `Solution Explorer`, find and right-click on the `MarkLogicAddIn` project file.
> - Select `Pro Fix References`.  This will update the project file with the correct local paths.
> - Rebuild the solution.

### Testing it out

To use the add-in you must have a valid installation of [ArcGIS Pro](https://pro.arcgis.com) of at least version 2.1.

Building the solution automatically registers the add-in on your local ArcGIS Pro application.

You can run ArcGIS Pro normally or start it via debugging `Debug -> Start Debugging` or pressing `F5` in Visual Studio.

Create a new project in ArcGIS Pro.  If the project has no map, add a new map.

Open the `MarkLogic` ribbon tab and click on `Search`.  This will open a new dock pane (to the right by default).

Enter a query in the search box, for example `ISIS or ISIL`.

The add-in will connect to the `esri-koop` MarkLogic server and will display the points from the search on the map.  If this 
is the first time the add-in will establish a connection, it will prompt the user for credentials.

Type in a layer name under `Save to New Layer` and press `Save`.  This will save the recently search results into a feature layer.

## Release

Build the solution using the `Release` configuration.

## Installation

The add-in can be deployed to a Windows machine, virtual or otherwise, with an existing installation of ArcGIS Pro.

### Single User

To deploy the add-in for a single user:

Download the zip file for the release you want to install (this would have been done by you or someone else when a release was created).

Create an ZIP/archive of all the files in the `bin/Release` folder.

Copy the archive to the destination machine and extract the files into a temporary directory.

Open Windows Explorer, navigate to the directory, and double-click on the `MarkLogic.Esri.ArcGISPro.Addin.esriAddinX`.

The `Esri ArcGIS Add-In Installation Utility` prompt will appear to confirm the installation.  Click `Install Add-In`.  

Open ArcGIS Pro.  Confirm the add-in installation by looking for the `MarkLogic` ribbon tab and trying out a search.

> This process will overwrite an older version of the add-in if it exists.

### Shared

The add-in can be deployed on a machine with multiple ArcGIS Pro users.  The process is similar to the above, with the exception that the add-in files must be publicly accessible by all of the machine's users.

#### Part 1

To setup the home location of the add-in for all machine users:

Download the zip file for the release you want to install (this would have been done by you or someone else when a release was created).

Create an ZIP/archive of all the files in the `bin/Release` folder.

Open Windows Explorer and navigate to your system disk (usually C:) and the directory path `Users/Public/Public Documents`.

Create directories that follow the path `ArcGIS/AddIns/arcgis-ml-addin`.

Copy the archive to the destination machine and extract the files into `ArcGIS/AddIns/arcgis-ml-addin`.  

> To update the add-in, simply create a new build, delete all files under `ArcGIS/AddIns/arcgis-ml-addin`, and extract the new files into the directory.

#### Part 2

To setup a machine user to use the add-in:

Open ArcGIS Pro.  In the main screen, select `Project` in the upper-left hand corner of the window.

Select `Add-In Manager` then open the `Options` tab.  Click on `Add Folder...` and select the `Users/Public/Public Documents/ArcGIS` folder.

ArcGIS Pro may indicate the need to restart for the changes to take effect.  If so, restart ArcGIS Pro.

Go back to `Add-In Manager` and verify that the `MarkLogic Search` add-in is present under Shared Add-Ins.







