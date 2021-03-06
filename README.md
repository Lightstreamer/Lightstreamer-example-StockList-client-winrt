# Lightstreamer - Basic Stock-List Demo - WinRT Client

<!-- START DESCRIPTION lightstreamer-example-stocklist-client-winrt -->

This project contains a demo client showing integration between [Lightstreamer .NET PCL Client](http://www.lightstreamer.com/api/ls-pcl-client/latest/) and the WinRT platform.

## Live Demo

[![screenshot](screen_wp_large.png)](http://apps.microsoft.com/webpdp/en-US/app/lightstreamer-stock-list-demo/8467d915-5499-44b7-8d20-93baaf916682)<br>
### [![](http://demos.lightstreamer.com/site/img/play.png) View live demo](http://apps.microsoft.com/webpdp/en-US/app/lightstreamer-stock-list-demo/8467d915-5499-44b7-8d20-93baaf916682)<br>

## Details

This is a WinRT (Windows Runtime for Windows 8) version of the [Stock-List Demos](https://github.com/Lightstreamer/Lightstreamer-example-Stocklist-client-javascript). desktop<br>
This app uses the <b>.NET PCL Client API for Lightstreamer</b> to handle the communications with Lightstreamer Server. A simple user interface is implemented to display the real-time data received from Lightstreamer Server.

In particular, this readme file details the steps required to build the demo on a Windows 8, 8.1, or 10 installation, using Visual Studio 2015.

### Dig the Code

* `App.xaml.cs` is the main application file; it contains application events handling code (application launching, closed, activated, etc.), Lightstreamer Client streaming controls (start, stop), and part of the auto-reconnection logic required when connection quality is weak.
* `MainPage.xaml.cs` is the place where all the application widgets are handled. The application, in fact, consists in a simple table containing real-time (simulated) stock quotes, whose cells are kept up-to-date by implementing the ILightstreamerListener interface.
Several lines in this file are dedicated to the animation code used for highlighting cells in case of real-time updates, you can ignore them altogether if you feel more comfortable.
* `LightstreamerClient.cs` is a simple wrapper for LSClient public class (see Lightstreamer .NET PCL Client API) and uses two listeners: `StocklistConnectionListener.cs` and `StocklistHandyTableListener.cs`, respectively implementing a Connection Status listener and a Real-Time Data Updates listener.
  
Check out the sources for further explanations.<br>

<i>NOTE: Not all the functionalities of the Lightstreamer .NET PCL Client Library are exposed by the classes listed above. You can easily expand those functionalities using the .NET PCL Client API as a reference.<br>  
If in trouble, check out the [Lightstreamer forum](http://forums.lightstreamer.com/).</i>

<!-- END DESCRIPTION lightstreamer-example-stocklist-client-winrt -->

## Install

If you want to skip the build process of this demo, please note that you can install and run the app from the Windows Store, either click the image or link above within Windows 8, 8.1 or 10, or go to the store and search for "Lightstreamer".<br>

## Build 

To build your own version of the demo, instead of using the one provided in the store as from the [Install](https://github.com/Lightstreamer/Lightstreamer-example-StockList-client-winrt#install) section above, follow these steps:

* To directly import the project as-is, you can use Visual Studio 2015. You can download it from the [Microsoft website](https://www.visualstudio.com/it/downloads/).<br>
* You may run the demo against your local server or using our online server at http://push.lightstreamer.com:80. The server to which the demo will connect to is configured in the `App.xaml.cs` file.<br>
  * In the former case, note that, as prerequisite, the [Lightstreamer - Stock- List Demo - Java Adapter](https://github.com/Lightstreamer/Lightstreamer-example-Stocklist-adapter-java) has to be deployed on your local Lightstreamer Server instance. Please check out that project and follow the installation instructions provided with it.
* You're now ready to import the project into Visual Studio, click on <b>File->New Project->Blank App</b>, rename the project *WinRTStockListDemo*, and import all the files located in the demo folder.
* You should complete this project with the Lightstreamer .NET PCL Client library, to be used for the build process, trough NuGet. Follow these steps:
	* In the "Solution Explorer" tab, right click on the project and choose `Manage NuGet Packages ...`
	* In the Search text box enter `Lightstreamer`
	* Choose Lightstreamer.DotNet.Client then click `Install` and then `Ok`
	* Check out that among the References of your project Lightstreamer_DotNet_PCL_Client was added.
* From Visual Studio, right-click on the *WinRTStockListDemo* project in the Solution Explorer menu and select <b>Debug->Start New Instance</b>. The WinRT app will be installed and launched.

## See Also

### Lightstreamer Adapters Needed by This Demo Client

<!-- START RELATED_ENTRIES -->
* [Lightstreamer - Stock-List Demo - Java Adapter](https://github.com/Lightstreamer/Lightstreamer-example-Stocklist-adapter-java)
* [Lightstreamer - Reusable Metadata Adapters- Java Adapter](https://github.com/Lightstreamer/Lightstreamer-example-ReusableMetadata-adapter-java)

<!-- END RELATED_ENTRIES -->

### Related Projects

* [Lightstreamer - Stock-List Demos - HTML Clients](https://github.com/Lightstreamer/Lightstreamer-example-Stocklist-client-javascript)
* [Lightstreamer - Basic Stock-List Demo - jQuery (jqGrid) Client](https://github.com/Lightstreamer/Lightstreamer-example-StockList-client-jquery)
* [Lightstreamer - Stock-List Demo - Dojo Toolkit Client](https://github.com/Lightstreamer/Lightstreamer-example-StockList-client-dojo)
* [Lightstreamer - Basic Stock-List Demo - Java SE (Swing) Client](https://github.com/Lightstreamer/Lightstreamer-example-StockList-client-java)
* [Lightstreamer - Basic Stock-List Demo - .NET Client](https://github.com/Lightstreamer/Lightstreamer-example-StockList-client-dotnet)
* [Lightstreamer - Stock-List Demos - Flex Clients](https://github.com/Lightstreamer/Lightstreamer-example-StockList-client-flex)
* [Lightstreamer - Basic Stock-List Demo - Silverlight Client](https://github.com/Lightstreamer/Lightstreamer-example-StockList-client-silverlight)

## Lightstreamer Compatibility Notes 

* Compatible with Lightstreamer .NET PCL Client Library version 3.0.0 or newer.
* Ensure that .NET PCL Client API is supported by Lightstreamer Server license configuration.
