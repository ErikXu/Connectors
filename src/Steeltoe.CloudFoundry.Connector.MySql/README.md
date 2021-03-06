﻿# CloudFoundry .NET MySql Connector

This project contains a Steeltoe Connector for MySql.  This connector simplifies using [Connector/NET - 6.9.9/7.0.x](https://dev.mysql.com/downloads/connector/net/) in an application running on CloudFoundry.

## Provider Package Name and Feeds

`Steeltoe.CloudFoundry.Connector.MySql`

[Development feed (Less Stable)](https://www.myget.org/gallery/steeltoedev) - https://www.myget.org/gallery/steeltoedev

[Master feed (Stable)](https://www.myget.org/gallery/steeltoemaster) - https://www.myget.org/gallery/steeltoemaster

[Release or Release Candidate feed](https://www.nuget.org/) - https://www.nuget.org/

## Usage
You probably will want some understanding of how to use the [Connector/NET - 6.9.9/7.0.x](https://dev.mysql.com/downloads/connector/net/) before starting to use this connector. Also basic understanding of the `ConfigurationBuilder` and how to add providers to the builder is also helpful.

In order to use this Connector you need to do the following:
```
1. Create and bind a MySql Service instance to your application.
2. Optionally, configure any MySql client settings (e.g. appsettings.json)
3. Add Steeltoe CloudFoundry config provider to your ConfigurationBuilder.
4. Add MySqlConnection or DbContext to your ServiceCollection.
```
## Create & Bind MySql Service
You can create and bind MySql service instances using the CloudFoundry command line (i.e. cf):
```
1. cf target -o myorg -s myspace
2. cf create-service p-mysql 100mb myMySqlService
3. cf bind-service myApp myMySqlService
4. cf restage myApp
```
Once you have bound the service to the app, the providers settings have been setup in `VCAP_SERVICES` and will be picked up automatically when the app is started by using the `CloudFoundry` configuration provider at startup.

## Optionally - Configure MySql Client Settings
Optionally you can configure the settings the Connector will use when setting up the MySqlConnection. Typically you would put these in your `appsettings.json` file and use the JSON configuration provider to add them to the applications configuration. Then when the MySql Connector configures the MySqlConnection it will the combine the settings from `appsettings.json` with the settings it obtains from the CloudFoundry configuration provider, with the CloudFoundry settings overriding any settings found in `appsettings.json`.

```
{
...
  "mysql": {
    "client": {
      "server": "myserver",
      "port": 3309
    }
  }
  .....
}
```

 
For a complete list of client settings see the documentation in the `MySqlProviderConnectorOptions` file.

## Add the CloudFoundry Configuration Provider
Next we add the CloudFoundry Configuration provider to the builder (e.g. `AddCloudFoundry()`). This is needed in order to pickup the VCAP_ Service bindings and add them to the Configuration. Here is some sample code illustrating how this is done:
```
#using Steeltoe.Extensions.Configuration;
...

var builder = new ConfigurationBuilder()
    .SetBasePath(basePath)
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()                   
    .AddCloudFoundry();
          
var config = builder.Build();
...

```
Normally in an ASP.NET Core application, the above C# code is would be included in the constructor of the `Startup` class. For example, you might see something like this:
```
#using Steeltoe.Extensions.Configuration;

public class Startup {
    .....
    public IConfigurationRoot Configuration { get; private set; }
    public Startup(IHostingEnvironment env)
    {
        // Set up configuration sources.
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()

            // Add to configuration the Cloudfoundry VCAP settings
            .AddCloudFoundry();

        Configuration = builder.Build();
    }
    ....
```

## Add MySqlConnector or a DbContext
The next step is to add MySqlConnector or DbContext's to your ServiceCollection depending on your needs.  You do this in `ConfigureServices(..)` method of the startup class:
```
#using Steeltoe.CloudFoundry.Connector.MySql;
... OR
#using Steeltoe.CloudFoundry.Connector.MySql.EF6;

public class Startup {
    .....
    public IConfigurationRoot Configuration { get; private set; }
    public Startup(...)
    {
      .....
    }
    public void ConfigureServices(IServiceCollection services)
    {
        // Add MySqlConnector configured from CloudFoundry
        services.AddMySqlConnection(Configuration);

        // OR 

        // If using EF6
        services.AddDbContext<TestContext>(Configuration);

        // OR

        // If using EFCore
        services.AddDbContext<TestContext>(options => options.UseMySql(Configuration));

        // Add framework services.
        services.AddMvc();
        ...
    }
    ....
```
## Using MySql
Below is an example illustrating how to use the DI services to inject a `MySqlConnector` or a `DbContext` into a controller:


```
using MySql.Data.MySqlClient;
....
public class HomeController : Controller
{
    public HomeController()
    {
    }
    ...
    public IActionResult MySqlData(
        [FromServices] MySqlConnection dbConnection)
    {
        dbConnection.Open();

        MySqlCommand cmd = new MySqlCommand("SELECT * FROM TestData;", dbConnection);
        MySqlDataReader rdr = cmd.ExecuteReader();

        while (rdr.Read())
        {
            ViewData["Key" + rdr[0]] = rdr[1];
        }

        rdr.Close();
        dbConnection.Close();

        return View();
    }
}

 
---------- If using EF6 ---------------
using MySql.Data.Entity;
using System.Data.Entity;
...

[DbConfigurationType(typeof(MySqlEFConfiguration))]
public class TestContext : DbContext
{
    public TestContext(string connectionString) : base(connectionString)
    {
    }
    public DbSet<TestData> TestData { get; set; }
}
 
---------- If using EFCore ---------------
using Microsoft.EntityFrameworkCore;
...

    public class TestContext : DbContext
    {
        public TestContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<TestData> TestData { get; set; }
    }

----------- Sample Controller code ----------
using Project.Models;
....
public class HomeController : Controller
{
    public HomeController()
    {
    }
    public IActionResult MySqlData(
        [FromServices] TestContext context)
    {

        var td = context.TestData.ToList();
        foreach (var d in td)
        {
            ViewData["Key" + d.Id] = d.Data;
        }

        return View();
    }

``` 
