@description('The location where we will deploy our resources to. Default is the location of the resource group')
param location string = resourceGroup().location

@description('Name of our application.')
param applicationName string = uniqueString(resourceGroup().id)

var logAnalyticsWorkspaceName = 'logs-${applicationName}'
var logAnalyticsWorkspaceSku = 'PerGB2018'
var appInsightsWorkspaceName = 'appsins-${applicationName}'
var containerRegistryName = 'acr${applicationName}'
var containerRegistrySkuName = 'Basic'
var containerEnvironmentName = 'env-${applicationName}'
var containerApiAppName = 'bookwormzapi'
var cosmosDbAccountName = 'cosmosdb-${applicationName}'
var bookDatabaseName = 'BooksDB'
var bookContainerName = 'Books'
var apimInstanceName = 'apim-${applicationName}'
var apimSkuName = 'Developer'
var publisherName = 'Will Velida'
var publisherEmail = 'willvelida@microsoft.com'

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    sku: {
      name: logAnalyticsWorkspaceSku
    }
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsWorkspaceName
  location: location 
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2021-12-01-preview' = {
  name: containerRegistryName
  location: location 
  sku: {
    name: containerRegistrySkuName
  }
  properties: {
    adminUserEnabled: true
  }
  identity: {
    type: 'SystemAssigned'
  }
}

resource containerEnvironment 'Microsoft.App/managedEnvironments@2022-03-01' = {
  name: containerEnvironmentName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  } 
}

resource apiContainerApp 'Microsoft.App/containerApps@2022-03-01' = {
  name: containerApiAppName
  location: location
  properties: {
    managedEnvironmentId: containerEnvironment.id
    configuration: {
      secrets: [
        {
          name: 'registrypassword'
          value: containerRegistry.listCredentials().passwords[0].value
        }
        {
          name: 'cosmosdbendpoint'
          value: cosmosDb.outputs.cosmosDBEndpoint
        }
        {
          name: 'databasename'
          value: cosmosDb.outputs.databaseName
        }
        {
          name: 'containername'
          value: cosmosDb.outputs.containerName
        }
        {
          name: 'appinsightsinstrumentationkey'
          value: appInsights.properties.InstrumentationKey 
        }
        {
          name: 'appinsightsconnectionstring'
          value: 'InstrumentationKey=${appInsights.properties.InstrumentationKey}'
        }
      ]
      registries: [
        {
          server: '${containerRegistry.name}.azurecr.io'
          username: containerRegistry.listCredentials().username
          passwordSecretRef: 'registrypassword'
        }
      ]
      ingress: {
        external: true
        targetPort: 80
        allowInsecure: false
      }
      activeRevisionsMode: 'multiple'
    }
    template: {
      containers: [
        {
          image: 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
          name: containerApiAppName
          resources: {
            cpu: '0.5'
            memory: '1.0Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 3
      }
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

module cosmosDb 'modules/cosmosDb.bicep' = {
  name: 'cosmosDb'
  params: {
    bookContainerName: bookContainerName
    bookDatabaseName: bookDatabaseName
    cosmosDbAccountName: cosmosDbAccountName
    location: location
  }
}

resource apim 'Microsoft.ApiManagement/service@2021-08-01' = {
  name: apimInstanceName
  location: location
  sku: {
    capacity: 1
    name: apimSkuName
  }
  properties: {
    publisherEmail: publisherEmail
    publisherName: publisherName
  }
  identity: {
    type: 'SystemAssigned'
  }
}
