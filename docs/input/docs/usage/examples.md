```c#
#addin "Cake.ResxConverter"

Task("Run")
  .Does(() =>
{
  // The path for the folder with resx files.
  var resxFolder = "src/Shared/Resources"; 
  
  // Convert the resx files to android into the specified folder.
  ResxConverter.ConvertToAndroid(resxFolder, "artifacts/generated/android");
  
  // Convert the resx files to ios into the specified folder.
  ResxConverter.ConvertToiOS(resxFolder, "artifacts/generated/ios");
});
```