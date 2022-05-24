@description('The name of the Cosmos DB Account that will be deployed.')
param cosmosDbAccountName string

@description('The location to deploy the Cosmos DB account to.')
param location string

@description('The name of the Database that will be deployed to our Cosmos DB account.')
param bookDatabaseName string

@description('The name of the Container that will be deployed to our Database.')
param bookContainerName string

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2021-11-15-preview' = {
  name: cosmosDbAccountName
  location: location
  properties: {
    databaseAccountOfferType: 'Standard' 
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    enableAnalyticalStorage: true
  }
  identity: {
    type: 'SystemAssigned'
  }
}

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2021-11-15-preview' = {
  name: bookDatabaseName
  parent: cosmosDbAccount
  properties: {
    resource: {
      id: bookDatabaseName
    }
  }
}

resource container 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2021-11-15-preview' = {
  name: bookContainerName
  parent: database
  properties: {
    resource: {
      id: bookContainerName
      indexingPolicy: {
        indexingMode: 'consistent'
        includedPaths: [
          {
            path: '/*'
          }
        ]
      }
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
      }
    }
    options: {
      autoscaleSettings: {
        maxThroughput: 4000
      }
    }
  }
}

output databaseName string = database.name
output containerName string = container.name
output cosmosDBEndpoint string = cosmosDbAccount.properties.documentEndpoint
