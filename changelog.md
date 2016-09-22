# ServiceAgents Toolbox

## 1.0.0

- initial version

## 1.1.0

- 

## 1.2.0

- Added ApiKey authentication feature

## 1.2.1

- Bugfix in Url creation in ServiceSettings
- Added GetStringAsync to Agentbase for string type responses

## 1.2.2

- Added customization of the api key header name

## 3.0.0

- Changed error handling to the standard Digipolis.Errors format

## 3.0.1

- Bugfix where empty request would throw a NullReferenceException

## 3.1.0

- Agents registred as Singleton instead of Scoped
- Added Basic authentication
- Refactored OAuthClientCredentials 

## 3.1.1

- Revert Agents registrations to Scoped
