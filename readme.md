# ServiceAgents Toolbox

Toolbox for ServiceAgents in ASP.NET Core.

## Table of Contents

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [Installation](#installation)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## Installation

To add the toolbox to a project, you add the package to the project.json :

``` json 
    "dependencies": {
        "Toolbox.ServiceAgents":  "1.0.0"
    }
``` 

In Visual Studio you can also use the NuGet Package Manager to do this.

## Usage


To add a single service agent in your project simply use the **AddSingleServiceAgent&lt;T>** extension in the **ConfigureServices** method in the **Startup** class.
The type represents the implementation of your service agent.
It will be registrated as a scoped instance. If the service agent implements an interface of the same name preceeded with an I, the interface will be registrated as scoped.

``` csharp
    services.AddSingleServiceAgent<YourServiceAgent>(settings =>
    {
        settings.Scheme = "http";
        settings.Host = "test.com";
        settings.Path = "api";
    });
```
The parameter of type **Action&lt;ServiceSettings>** allows you to set the settings for the service agent.

If you want to configure the agent using a configuration file or if you want to register multiple service agents use the **AddServiceAgents** extention instead.

``` csharp
    services.AddServiceAgents(settings =>
    {
        settings.FileName = "serviceagents.json";
    });
```

The json file contains one or multiple sections, one per service agent.

The structure of the json file is:

``` javascript
{
    "TestAgent": {
      "AuthScheme": "None",
      "Host": "test.be",
      "Path": "api",
      "Port": "5001",
      "Scheme": "http"
    }
}
```

Each object (section) in the json represents a service agent type. The object name has to match the service agent type name.
See section creating a service agent for more info on creating the service agents.

Following options can be set per section (service agent):

Option              | Description                                                | Default
------------------ | ----------------------------------------------------------- | --------------------------------------
AuthScheme              | The authentication scheme to be used by the service agent. | "None"
Host | The host part of the url for the service agent. | ""
Path | The path part of the url for the service agent. | "api" 
Port | The port for the service agent. | "80" 
Scheme | The scheme of the url for the service agent. | "https"   

The settings without default are manadatory!
All the url parts form the basic url for the service agent: {scheme}://{host}:{port}/{path}

An overload for both **AddSingleServiceAgent&lt;T>** and **AddServiceAgents** is available where you can pass an action of type **Action&lt;HttpClient>** that gets invoked when the underlaying HttpClient gets created. That way you can customize the created client.

Important notice: the action gets invoked for every service agent when multuple are registrated!
``` csharp
    services.AddServiceAgents(s =>
    {
        s.FileName = "serviceagents.json";
        s.Section = "TestAgent";
    }, 
    client => 
    {
        //customize the client
    });
```


## Creating a service agent

In order to create a service agent you need to create a type that derives from **AgentBase**.

``` csharp
    public class DemoAgent : AgentBase
    {
        public DemoAgent(IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options) 
            : base(serviceProvider, options, "DemoAgent")
        {
        }
    }
```

The name of the type has to be passed as the "key" parameter to the base class constructor.

The **AgentBase** class contains several protected methods to perform the basic http actions (get, post, put and delete). All the methods are async. 

Some examples:

Implement a get operation

``` csharp
    public class DemoAgent : AgentBase
    {
        public DemoAgent(IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options) 
            : base(serviceProvider, options, "DemoAgent")
        {
        }

        //A basic get operation
        public Task<Address> GetAddressAsync(int id)
        {
            return base.GetAsync<Address>($"adress?id={id}");
        }

        //A basic post operation
        public Task<Address> PostAddressAsync(Address adress)
        {
            return base.PostAsync<Address>("adress", adress);
        }
    }
``` 

## Using a service agent

In order to use a service agent simply ask the **serviceProvider** for an instance of the agent's type and use it.

For example in a controller:

 ``` csharp
    public class ValuesController : Controller
    {
        private DemoAgent _serviceAgent;

        public ValuesController(DemoAgent serviceAgent)
        {
            _serviceAgent = serviceAgent;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            var result = await _serviceAgent.GetAddressAsync(1);
            
            //do something...
            
            return ...;
        }
    }
```

If your agent implements an interface with the same name preceeded with an **I**, the agent is registrated as an implementation of that interface.
In that case you need to request an instance of the interface type:

``` csharp
    //The service agent implementation
    public class DemoAgent : AgentBase, IDemoAgent
    {
        public DemoAgent(IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options) 
            : base(serviceProvider, options, "DemoAgent")
        {
        }

        //Some implementation
    }
    
    //The usage
    public class ValuesController : Controller
    {
        private IDemoAgent _serviceAgent;

        public ValuesController(IDemoAgent serviceAgent)
        {
            _serviceAgent = serviceAgent;
        }
    }
```