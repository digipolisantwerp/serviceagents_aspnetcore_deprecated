# ServiceAgents Toolbox

Toolbox for ServiceAgents in ASP.NET Core.

## Table of Contents

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->


- [Installation](#installation)
- [Usage](#usage)
- [Creating a service agent](#creating-a-service-agent)
- [Using a service agent](#using-a-service-agent)
- [Authentication schemes](#authentication-schemes)
  - [None](#none)
  - [Bearer](#bearer)
  - [ApiKey](#apikey)
  - [OAuthClientCredentials](#oauthclientcredentials)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## Installation

To add the toolbox to a project, you add the package to the project.json :

``` json
    "dependencies": {
        "Digipolis.ServiceAgents":  "2.0.0"
    }
```

ALWAYS check the latest version [here](https://github.com/digipolisantwerp/serviceagents_aspnetcore/blob/master/src/Digipolis.ServiceAgents/project.json) before adding the above line !

In Visual Studio you can also use the NuGet Package Manager to do this.

## Usage

To add a single service agent in your project simply use the **AddSingleServiceAgent&lt;T>** extension in the **ConfigureServices** method in the **Startup** class.
The type represents the implementation of your service agent.
It will be registered as a scoped instance. If the service agent implements an interface of the same name preceded with the letter ‘I’, the interface will be registered as scoped.

``` csharp
    services.AddSingleServiceAgent<YourServiceAgent>(settings =>
    {
        settings.Scheme = "http";
        settings.Host = "test.com";
        settings.Path = "api";
    });
```
The parameter of type **Action&lt;ServiceSettings>** allows you to set the settings for the service agent.

If you want to configure the agent using a configuration file or if you want to register multiple service agents use the **AddServiceAgents** extension instead.

``` csharp
    services.AddServiceAgents(settings =>
    {
        settings.FileName = "serviceagents.json";
    });
```

If your agents are located in a different assembly then the executing assembly, you can pass that assembly as a parameter, this is optional.

``` csharp
    services.AddServiceAgents(settings =>
    {
        settings.FileName = "serviceagents.json";
    },
	assembly: AssemblyWithAgents);
```

The json file contains one or multiple sections, one per service agent.

The structure of the json file is:

``` javascript
{
    "Global": {
    "GlobalApiKey": "globalapikey"
    },
    "TestAgent": {
      "AuthScheme": "None",
      "Host": "test.be",
      "Path": "api",
      "Port": "5001",
      "Scheme": "http",
      "UseGlobalApiKey": false,
      "ApiKey": "localapikey"
    }
}
```

A first object (section) section named **Global** can optionally be defined.
In this section a **GlobalApiKey** option can be set. See the [Authentication schemes](#authentication-schemes) section for more details.

Each other object (section) in the json represents a service agent type. **The object name has to match the service agent type name**.
See the [Creating a service agent](#creating-a-service-agent) section for more info on creating the service agents.

Following options can be set per section (service agent):

Option              | Description                                                | Default
------------------ | ----------------------------------------------------------- | --------------------------------------
AuthScheme              | The authentication scheme to be used by the service agent. | "None"
Host | The host part of the url for the service agent. | ""
Path | The path part of the url for the service agent. | "api"
Port | The port for the service agent. | ""
Scheme | The scheme of the url for the service agent. | "https"
UseGlobalApiKey | A Boolean to indicate to use the globally defined api key for authentication. | false
ApiKey | The locally defined api key for authentication. | ""
ApiKeyHeaderName | The string used as header name for the api key. | "ApiKey"

The settings without default are mandatory!
All the url parts form the basic url for the service agent: {scheme}://{host}:{port}/{path}/

An overload for both **AddSingleServiceAgent&lt;T>** and **AddServiceAgents** is available where you can pass an action of type **Action&lt;IServiceProvider, HttpClient>** that gets invoked when the underlying HttpClient gets created. That way you can customize the created client.

Important notice: the action gets invoked for every service agent when multiple are registered!
``` csharp
    services.AddServiceAgents(s =>
    {
        s.FileName = "serviceagents.json";
        s.Section = "TestAgent";
    },
    (serviceProvider, client) =>
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
            : base(serviceProvider, options)
        {
        }
    }
```

The **AgentBase** class contains several protected methods to perform the basic http actions (get, post, put and delete). All the methods are async.

Some examples:

Implement a get operation

``` csharp
    public class DemoAgent : AgentBase
    {
        public DemoAgent(IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options)
            : base(serviceProvider, options)
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

If you want to return the response as string you can use the **GetStringAsync** method on the **Agentbase**.

``` csharp
    public Task<string> GetAddressAsStringAsync(int id)
    {
        return base.GetStringAsync($"adress?id={id}");
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

            return "somestring";
        }
    }
```

If your agent implements an interface with the same name preceded with an **I**, the agent is registered as an implementation of that interface.
In that case you need to request an instance of the interface type:

``` csharp
    //The service agent implementation
    public class DemoAgent : AgentBase, IDemoAgent
    {
        public DemoAgent(IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options)
            : base(serviceProvider, options)
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

## Authentication schemes

Different schemes are available to be used with the service agents.

Possible schemes:

* None
* Bearer
* ApiKey
* OAuthClientCredentials

Check the [Creating a service agent](#creating-a-service-agent) section to see how to define the scheme for your service agents.

### None

No authentication is done.

### Bearer

With the Bearer scheme the authentication is done through use of the authorization header:

    Authorization Bearer xxx
where xxx is the token.
To use the Bearer scheme set the **AuthScheme** property of the **ServiceSettings** object to the value "Bearer".
The token is extracted from the **UserToken** property of the **AuthContext** object provided by the dependency injection infrastructure.

### ApiKey

With the ApiKey scheme the authentication is done through use of the apikey header:

    ApiKey yyy
where yyy is the api key.  
It is possible to override the default header name by setting an other value in the **ApiKeyHeaderName** property of the service settings.

To use the ApiKey scheme set the **AuthScheme** property of the **ServiceSettings** object to the value "ApiKey".

If multiple or all agents use the same api key to authenticate you can set the key in the **GlobalApiKey** field of the **Global** section in the json config file.
This in combination with the **UseGlobalApiKey** in the service agent section.

In the example below two agents that use the same key are configured.

``` javascript
{
  "Global": {
    "GlobalApiKey": "apikeyformultipleagents"
  },
  "FirstAgent": {
    "Host": "test.be",
    "Path": "api",
    "Port": "5001",
    "Scheme": "http",
    "AuthScheme": "ApiKey",
    "UseGlobalApiKey": true
  },
  "SecondAgent": {
    "Host": "test.be",
    "Path": "api",
    "Port": "5001",
    "Scheme": "http",
    "AuthScheme": "ApiKey",
    "UseGlobalApiKey": true
  }
}
```

If you have different keys for your service agents, set the **UseGlobalApiKey** to 'false' and set the key in the **ApiKey** field of the service agent section.

In the example below two agents that use different keys are configured.
``` javascript
{
  "FirstAgent": {
    "Host": "test.be",
    "Path": "api",
    "Port": "5001",
    "Scheme": "http",
    "AuthScheme": "ApiKey",
    "UseGlobalApiKey": false,
    "ApiKey": "apikeyforfirstagent"
  },
  "SecondAgent": {
    "Host": "test.be",
    "Path": "api",
    "Port": "5001",
    "Scheme": "http",
    "AuthScheme": "ApiKey",
    "ApiKeyHeaderName": "api-key",
    "UseGlobalApiKey": false,
    "ApiKey": "apikeyforsecondagent"
  }
}
```

If you don't want to enter your api keys in the json configuration file you can use an overload of the **AddServiceAgents** method that accepts a parameter of type **Action&lt;ServiceAgentSettings&gt;**
to alter the values of the service settings after they have been loaded from the json file.

``` csharp
    services.AddServiceAgents(json =>
    {
        json.FileName = "_TestData/serviceagentconfig_1.json";
    }, serviceAgentSettings =>
    {
        serviceAgentSettings.GlobalApiKey = "globalkeyfromcode";
    }, null);
 ```

### OAuthClientCredentials

This will use the OAuth2 client credentials flow to obtain a (bearer)token from a token endpoint.

To use the OAuth Clientcredentials scheme set the **AuthScheme** property of the **ServiceSettings** object to the value "OAuthClientCredentials".

You must also supply following settings in the ServiceSettings:

```csharp
  OAuthClientId = "f44d3641-8249-440d-a6e5-61b7b4893184";
  OAuthClientSecret = "2659485f-f0be-4526-bb7a-0541365351f5";
  OAuthScope = "testoauthDigipolis.v2.all";
  OAuthPathAddition = "oauth2/token";
```

See the SampleApi for more info.
