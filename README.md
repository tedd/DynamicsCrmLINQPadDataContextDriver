# Dynamics CRM LINQPad DataContext Driver
LINQPad driver for Dynamics CRM.

# Releases
Current version is beta 1. It has only been tested on Dynamics CRM 2016 on-premise.
Download binary package: https://github.com/tedd/DynamicsCrmLINQPadDataContextDriver/releases

# How it works
Uses CrmSvcUtil.exe to generate a proxy. This should ensure compatibility with most CRM versions and setups.

# Why another driver?
The other drivers I tried did not work, and frankly I was not sure what they were trying to do. I just needed a straightforward way to connect to Dynamics CRM 2016 on-premise. A simple direct exposure to the connection parameters I use in code.

# What if it doesn't work?
Let me know by filing a bug report: https://github.com/tedd/DynamicsCrmLINQPadDataContextDriver/issues
Also, you have the source code. Feel free to fix it and push back a patch to me.

## Set up dev environment
  * Delete the drivers from LINQPad if you have installed the binary version.
  * Download the source code and compile it.
    * NuGET packages should download
	* Ensure that the reference to LINQPad.exe under project References is valid, if not, add reference to your LINQPad installation, i.e. C:\Program Files\LINQPad5\LINQPad.exe (yes, reference the .exe).
	* If you get Reference errors for Xrm assemblies you can reference all .dll assemblies under src\packages\Microsoft.CrmSdk.CoreTools.8.2.0.5\content\bin\coretools
  * Start LINQPad. 
  * You should now see the driver in LINQPad.
  * For every recompile: Close LINQPad, recompile, start LINQPad.

## Debugging tips
Even though "Attach to process" (CTRL+ALT+P) and "Reattach to process" (VS2017: SHIFT+ALT+P) is great, and helps with breakpoints and all, there are a few extra tricks.

  * "%localappdata%\LINQPad\Logs" contains a log-file from LINQPad.
  * On post-build event the driver will copy to "%localappdata%\LINQPad\Drivers\DataContext\4.6\Tedd.DynamicsCrmLINQPadDataContextDriver (165ac21ee0898778)\". CrmSvcUtil.exe will drop a log file here. This is also where cached proxies are.
  * Due to how LINQPad works, a breakpoint may not always break. Simply add "Debug.Break();" in your code if you need to break somewhere. It will stop and ask you to attach Visual Studio. Remember to remove this line after, or end users will also be asked to debug (and terminate process if they say no).
 
## Important points in the code
  * LINQPad\Astoria\AstoriaDynamicDriver.cs is the LINQPad driver entrypoint.
  * Models\ConnectionData.cs contains the configuration.
  * ViewModels\ConnectionDialogViewModel.cs is what is set as DataContext for Views\ConnectionDialog.xaml which is opened from LINQPad\Astoria\AstoriaDynamicDriver.cs
  * LINQPad\Astoria\SchemaBuilder.cs is where CRM proxy is generated by calling CrmSvcUtil.exe, the code it generates is compiled and the assembly loaded and analyzed.
  * DataConnection\XrmConnection.cs is used when you click "Connect" button on ConnectionDialog.xaml to verify your connection. This is strictly not required, just added as a convenience of not having to wait for CrmSvcUtil.exe to finish before knowing if everything is ok.
  