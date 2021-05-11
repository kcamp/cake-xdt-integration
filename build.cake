//#addin "nuget:?package=Cake.XdtTransform&version=1.0.0&loaddependencies=true"
#addin "nuget:file://C:/.nugetpackages/?package=Cake.XdtTransform&version=1.0.1-GH-46-0001&loaddependencies=true"

var sourceFile = File("web.config");
var transformFile = File("web.release.config");
var targetFile = File("web.target-default.config");

Task("Run-Default-Transform")
  .Does(() => {    
    Information("Running Default Transform");
    
    XdtTransformConfig(sourceFile, transformFile, targetFile);
    AssertMatch(targetFile, "transformed-value");
  });

Task("Run-Transform-With-Default-Logger")
  .Does(() => {
    Information("Running Transform With Default Logger");
    
    var log = XdtTransformConfigWithDefaultLogger(sourceFile, transformFile, targetFile);
    AssertMatch(targetFile, "transformed-value");
    AssertLogger(log);
  });

Task("Run-Transform-With-Custom-Logger")
  .Does(() => {
    Information("Running Transform With Custom Logger");
    var log = new XdtTransformationLog();
    XdtTransformConfigWithLogger(sourceFile, transformFile, targetFile, log);
    AssertMatch(targetFile, "transformed-value");
    AssertLogger(log);
  });

Task("Run-Transform-With-Null-Logger")
  .Does(() => {
    Information("Running Transform With 'null' Logger");
    // this should be quietly handled; the inner implementation tolerates 'null'
    XdtTransformConfigWithLogger(sourceFile, transformFile, targetFile, null);
    AssertMatch(targetFile, "transformed-value");
  });

Task("Run-Transform-With-Settings")
  .Does(() => {
    Information("Running default transform with settings");
    var log = new XdtTransformationLog();
    var settings = new XdtTransformationSettings().UseLogger(log);

    XdtTransformConfig(sourceFile, transformFile, targetFile, settings);
    AssertMatch(targetFile, "transformed-value");
    AssertLogger(log);
  });

Task("Run-Transform-With-Adhoc-Settings")
  .Does(() => {
    Information("Running default transform with settings");
    var log = new XdtTransformationLog();
    var settings = new XdtTransformationSettings().UseLogger(log);
    var transformFragment = "<appSettings><add key=\"variable-name\" value=\"adhoc-value\" xdt:Locator=\"Match(key)\" xdt:Transform=\"SetAttributes\" /></appSettings>";
    
    XdtTransformConfig(sourceFile, targetFile, new XdtFragmentSource(transformFragment), settings);
    AssertMatch(targetFile, "adhoc-value");
    AssertLogger(log);
  });  

Task("Default")
  .IsDependentOn("Run-Default-Transform")
  .IsDependentOn("Run-Transform-With-Default-Logger")
  .IsDependentOn("Run-Transform-With-Custom-Logger")
  .IsDependentOn("Run-Transform-With-Null-Logger")
  .IsDependentOn("Run-Transform-With-Settings")
  .IsDependentOn("Run-Transform-With-Adhoc-Settings");

RunTarget("Default");

void AssertLogger(XdtTransformationLog log)
{
  if(log.Log.Count() < 1)
  {
    throw new CakeException("No log entries were recorded");
  }

  Information("Log entries were recorded:");
  foreach(var item in log.Log)
  {
    Information(item);
  }
}
void AssertMatch(FilePath transformedFile, string substring)
{
  AssertMatch(transformedFile, s => s.IndexOf(substring, StringComparison.OrdinalIgnoreCase) > -1);
}
void AssertMatch(FilePath transformedFile, Func<string,bool> predicate)
{
  var lines = Context.FileSystem.GetFile(transformedFile).ReadLines(Encoding.UTF8);
  if(!lines.Any(predicate))
  {
    throw new CakeException("No value matching predicate was located");
  }
  
  Information("Predicate match was located.");
  Context.FileSystem.GetFile(transformedFile).Delete();
}