# Azure App Configuration
Consists of Configuration Management and Feature Management and has the following properties:
- Security using role based access control
- Store secrets using Azure Key Vault integration
- .NET Framework/Core integration
- Realtime updates without having to restart the app
- Decoupling of configuration from deployment

## Configuration Management
- Centralized configuration
- Change history for recovering from mistakes.

## Feature Management
- User targeting
- Enables Trunk-based Development
- Release anytime
- Test in production
- Progressive delivery
- Kill switch

## Architecture

### Dynamic Configuration: Pull model
``` mermaid
graph TD
    webapp[Web App]
    webapi[Web Api]
    appc[Azure App Config]
    appcu[Azure App Config User]
    webapp -->|Get features| webapi
    webapp -->|Get settings| webapi
    webapi -->|Poll config every N time| appc
    appc -->|Return latest config| webapi
    appcu -->|Change config| appc
    appc
```

### Dynamic Configuration: Push model
``` mermaid
graph TD
    webapp[Web App]
    webapi[Web Api]
    appc[Azure App Config]
    sb[Service Bus]
    appcu(Azure App Config User)
    azfun[Azure Function]
    sigr[SignalR Service]
    webapp -->|Get features| webapi
    webapp -->|Get settings| webapi
    appcu -->|Change config| appc
    webapi -->|Get config| appc
    appc .->|Return latest config| webapi
    appc -->|Config changed| sb
    sb .->|Notify subscribers of config change| webapi
    sb -->|Notify subscribers of config change| azfun
    azfun -->|Add message to hub| sigr
    sigr .->|Notify listeners| webapp
    webapp -->|Listen for config changes| sigr
```