# ServiceAgents Toolbox

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


