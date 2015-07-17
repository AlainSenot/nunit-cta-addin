DEL *.nupkg 2>NUL
.nuget\NuGet.exe setapikey admin:eikonTest12 -Source http://package.test.compass.int.thomsonreuters.com:81/nuget/etap%1
.nuget\NuGet.exe pack CTA.NUnitAddin.nuspec
.nuget\NuGet.exe push *.nupkg -s http://package.test.compass.int.thomsonreuters.com:81/nuget/etap%1
