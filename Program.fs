open Farmer
open Farmer.Builders
open Farmer.CosmosDb

let createEnvironment
        (applicationName:string, webAppSku, numberOfWorkers,
         repositoryUrl, branch,
         databaseName:string, containerName) =

    let theDatabase = cosmosDb {
        name databaseName
        account_name (applicationName + "-cosmos")
        consistency_policy Session
    }

    let theWebApp = webApp {
        name applicationName
        sku webAppSku
        number_of_workers numberOfWorkers
        settings [
            "CosmosDb:Account", theDatabase.Endpoint
            "CosmosDb:Key", theDatabase.PrimaryKey
            "CosmosDb:DatabaseName", theDatabase.DbName.Value
            "CosmosDb:ContainerName", containerName
        ]
        source_control repositoryUrl branch
        disable_source_control_ci
        depends_on theDatabase
    }

    arm {
        location Location.WestEurope
        add_resources [
            theDatabase
            theWebApp
        ]
    }

let deployment =
    createEnvironment ("isaac-to-do-app", WebApp.Sku.B1, 1,
                       "https://github.com/Azure-Samples/cosmos-dotnet-core-todo-app.git", "master",
                       "Tasks", "Items")

deployment
|> Deploy.execute "my-resource-group" Deploy.NoParameters
|> printfn "%A"