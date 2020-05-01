# MarkLogic ArcGIS Pro Addin

This project aims to provide Esri's [ArcGIS Pro](https://pro.arcgis.com) the capability to access and search a 
MarkLogic database straight from within the desktop application.  It is built using the .NET platform and the 
[ArcGIS Pro SDK](https://pro.arcgis.com/en/pro-app/sdk/).

## Prerequisites

The add-in requires the following to operate:

- A [Koop](https://github.com/koopjs/koop-provider-marklogic) server running the MarkLogic provider

> The add-in relies on Koop to serve features and act as a proxy to the MarkLogic database backend.  Please refer 
> to the [Working with the ArcGIS Pro add-in](https://github.com/koopjs/koop-provider-marklogic/tree/master#working-with-the-arcgis-pro-add-in)
> section for details on configuration and setup.

## Installation

The add-in can be deployed to a Windows machine, virtual or otherwise, with an existing installation of ArcGIS Pro. It can be install for a single user or as shared for all users (more common).

### Single User

To deploy the add-in for a single user:

- Download the zip file for the [release](https://github.com/marklogic-community/marklogic-arcgis-pro-addin/releases) you want to install (this would have been done by you or someone else when a release was created).
- Copy the archive to the destination machine and extract the files into a temporary directory.
- Open Windows Explorer, navigate to the directory, and double-click on the file `MarkLogic.Esri.ArcGISPro.Addin.esriAddinX`.
- The **Esri ArcGIS Add-In Installation Utility** prompt will appear to confirm the installation.  Click `Install Add-In`.  
- Open ArcGIS Pro.  Confirm the add-in installation by looking for the **Add-in** ribbon tab containing the **MarkLogic** tab group.

> Note: This process will overwrite an older version of the add-in if it exists.

### Shared Use

The add-in can be deployed on a machine for multiple ArcGIS Pro users.  This process installs the add-in to a location that makes the add-in files publicly accessible for all of the machine's users.

#### _Part 1_

To setup the home location of the add-in for all machine users:

- Download the zip file for the [release](https://github.com/marklogic-community/marklogic-arcgis-pro-addin/releases) you want to install (this would have been done by you or someone else when a release was created).
- Open Windows Explorer and navigate to your system disk (usually C:) and the directory path `Users/Public/Public Documents`.
- Create directories that follow the path `ArcGIS/AddIns/arcgis-ml-addin`.
- Copy the archive to the destination machine and extract the files into `ArcGIS/AddIns/arcgis-ml-addin`.  

> To update the add-in, simply download a new build, delete all files under `ArcGIS/AddIns/arcgis-ml-addin`, and extract the new files into the directory.

#### _Part 2_

To setup a machine user to use the add-in:

- Open ArcGIS Pro.  In the main screen, select `Project` in the upper-left hand corner of the window.
- Select `Add-In Manager` then open the `Options` tab.  Click on `Add Folder...` and select the `Users/Public/Public Documents/ArcGIS` folder.
- ArcGIS Pro may indicate the need to restart for the changes to take effect.  If so, restart ArcGIS Pro.
- Go back to `Add-In Manager` and verify that the **MarkLogic** tab group is present in the **Add-in** ribbon tab.

### MarkLogic Configuration

Koop relies on [Geo Data Services](https://github.com/marklogic-community/marklogic-geo-data-services) for accessing data stored in MarkLogic.  Additional configuration is required to enable 
it to work with the add-in.  Refer to this [page](https://github.com/marklogic-community/marklogic-geo-data-services/wiki/Working-with-the-ArcGIS-Pro-add-in) for more details on how the add-in interacts with Geo Data Services 
and the required changes to service descriptors.

### Server Registration

You will need to register your Koop server before the add-in can connect, if you have not done so before.

- Open ArcGIS Pro.  In the main screen, select `Project` in the upper-left hand corner of the window.
- Select `Options` and then the `MarkLogic` tab.  
- Under `Servers`, click on `Add...`.
- Fill up `Name` with a description or name of your server, e.g. "My Koop Server".
- Fill up `Host` and `Port` with your Koop server's host and port.  Check `Use SSL` if you have it enabled.
- Press `OK` to add your server to the list, and `OK` again to save your settings.
- Head back to your project and open the `MarkLogic Search` dock pane.  Your server should now be listed under `Connect to a server`.

### Connecting to a server

You must use your MarkLogic user credentials when prompted to login.

## Development

Follow these instructions if you want to develop new capabilities for the add-in.

### Dependencies

This solution has the following external dependencies:

- [ArcGIS Pro SDK for .NET](https://github.com/Esri/arcgis-pro-sdk/wiki/ProGuide-Installation-and-Upgrade)
- [Json.NET](https://www.newtonsoft.com/json)
- .NET Framework 4.6.1

Download and install [Visual Studio 2019](https://www.visualstudio.com/vs/community/) (Community or higher edition).
Ensure that at least the **.NET desktop development** workload is selected when choosing your installation configuration.

Install both [ArcGIS Pro SDK for .NET](https://github.com/Esri/arcgis-pro-sdk/wiki/ProGuide-Installation-and-Upgrade)
Visual Studio Extensions by following the instructions [here](https://github.com/Esri/arcgis-pro-sdk/wiki/ProGuide-Installation-and-Upgrade).

### Building the Project

#### via Visual Studio
Open the `arcgis-ml-addin.sln` solution file in Visual Studio.

Under the `Build` menu, select `Rebuild Solution`.

> Note: The ArcGIS Pro SDK assembly references persist themselves as absolute paths in the .csproj project file.
> If you encounter unresolved assembly reference errors when building, this can be remedied by taking the following steps:
> - In `Solution Explorer`, find and right-click on the `MarkLogicAddIn` project file.
> - Select `Pro Fix References`.  This will update the project file with the correct local paths.
> - Rebuild the solution.

### Testing it out

To use the add-in you must have a valid installation of [ArcGIS Pro](https://pro.arcgis.com) of at least version 2.4.

Building the solution automatically registers the add-in on your local ArcGIS Pro application.

You can run ArcGIS Pro normally or start it via debugging `Debug -> Start Debugging` or pressing `F5` in Visual Studio.

Create a new project in ArcGIS Pro.  If the project has no map, add a new map.

Open the `Add-In` ribbon tab located at the top of the application and click on `Search`.  This will open a new dock pane called `MarkLogic Search` (to the right by default).

Enter a query in the search box, for example `ISIS or ISIL`.

The add-in will connect to MarkLogic (proxied through Koop) and will display the points from the search on the map.  If this 
is the first time the add-in will establish a connection, it will prompt the user for credentials.

Type in a layer name under `Save to New Layer` and press `Save`.  This will save the recently search results into a feature layer.

### Release

Build the solution using the `Release` configuration.

### Notes on Debugging

Sometimes you wish to inspect the HTTP requests the add-in makes while debugging.  Web debugging proxy tools like [Telerik Fiddler](https://www.telerik.com/fiddler) are useful
in this situation, but won't always work with .NET apps ([read here](https://docs.telerik.com/fiddler/Configure-Fiddler/Tasks/ConfigureDotNETApp)) without making application or 
machine-level configuration changes.  This usually happens if the app is executed using an account different from the current user, which applies to the add-in.  Regardless of 
other settings, .NET will generally bypass proxies if the URL has `localhost`.

To enable the web proxy to intercept the add-in's requests, the best workaround (with least system impact) is to use your local computer's name in lieu of `localhost` as **host**
when registering your MarkLogic app server in the add-in.







