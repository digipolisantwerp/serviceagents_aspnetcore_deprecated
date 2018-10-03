# ServiceAgents Toolbox

## 6.0.6
- Expose response from an HttpClient call to classes that inherit AgentBase (useful when for ex. returned data model has not a fixed format )

## 6.0.5
- RequestHeaders are evaluated before each service agent request. If an OAuth access token has expired, a new one will be requested.

## 6.0.4
- AgentBase: initialization of _client (HttpClient) happens on first use instead of during constructor execution to avoid unnecessary calls to retrieve an access token for OAuth-authentication scheme

## 6.0.3
- ParseJsonError: fixed Exception handling - ExtraParameters always null
- OnParseJsonErrorException: Added hook for custom exception handling
- Ensure Statuscode is included

## 6.0.2
 - Bugfix : Improved error mapping
 - Bugfix : Added Content-Type: application/x-www-form-urlencoded header to oauth content to fix issues after Kong upgrade 

## 6.0.1
 - Bugfix : Added missing mapping of Code and title of validationexception

## 6.0.0

- Changed default value of path parameter from "api" to empty string
- Fixed exception handling for Digipolis error model with empty extraParameters value

## 5.1.3

- AgentBase throws ForbiddenException when HTTP status code is 403

## 5.1.2

- Bugfix: Registering agents with inherited base agent

## 5.1.1

- Bugfix: Add JSON body to Messages when extraParameters is null

## 5.1.0

- Added http patch to AgentBase

## 5.0.0

- conversion to csproj and MSBuild.
- update package System.Runtime.Serialization.Formatters to 4.3.0

## 4.1.0

- Added optional domain setting for Basic authentication

## 4.0.3

- Fixed Error object changes
 
## 4.0.2

- Updated Error toolbox version

## 4.0.1

- Allow use of basic Authentication with http scheme in development environment 

## 4.0.0

- Removed ApiKey authentication scheme
- Added Headers support

## 3.1.1

- Revert Agents registrations to Scoped

## 3.1.0

- Agents registred as Singleton instead of Scoped
- Added Basic authentication
- Refactored OAuthClientCredentials 

## 3.0.1

- Bugfix where empty request would throw a NullReferenceException


## 3.0.0

- Changed error handling to the standard Digipolis.Errors format

## 1.2.2

- Added customization of the api key header name

## 1.2.1

- Bugfix in Url creation in ServiceSettings
- Added GetStringAsync to Agentbase for string type responses

## 1.2.0

- Added ApiKey authentication feature

## 1.0.0

- initial version


