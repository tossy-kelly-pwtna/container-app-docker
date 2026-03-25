# container-app-docker
https://learn.microsoft.com/en-us/azure/container-apps/tutorial-deploy-from-code?source=recommendations&tabs=csharp

install .net sdk 
https://dotnet.microsoft.com/download

#Create the local application
//Create and run your source code.
dotnet new webapp --name MyAcaDemo --language C#
cd MyAcaDemo

//Open Program.cs in a code editor and replace the contents with the following //code.
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls("http://*:8080");
            });
}

//Implementing the Program class with this code creates the basis of a web //application. Next, create a class responsible for returning a web page as a //response.

//In the same folder, create a new file named Startup.cs and enter the following //code.
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
    }

    public void Configure(IApplicationBuilder app)
    {   
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        });
    }
}

//Now when a request is made to your web application, the text "Hello World!" is //returned. To verify your code is running correctly on your local machine, build //your project in release configuration.
dotnet build -c Release

//Next, run your application to verify your code is implemented correctly.
dotnet run --configuration Release

//Once you verify the application works as expected, you can stop the local //server and move on to creating a Dockerfile so you can deploy the app to //Container Apps.

//In the MyAcaDemo folder, create a file named Dockerfile and add the following //contents.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MyAcaDemo.dll"]

//Now that you have your code and a Dockerfile ready, you can deploy your app to //Azure Container Apps.

#Create Azure resources
//Sign in to Azure from the CLI with the following command. To complete the //authentication process, make sure to follow all the prompts.
az login

//Install or update the Azure Container Apps extension for the Azure CLI.
az extension add --name containerapp --upgrade

//Now that your Azure CLI setup is complete, you can define a set of environment //variables.
LOCATION="CentralUS"
RESOURCE_GROUP="my-demo-group"
IDENTITY_NAME="my-demo-identity"
ENVIRONMENT="my-demo-environment"
REGISTRY_NAME="mydemoregistry$(openssl rand -hex 4)"
CONTAINER_APP_NAME="my-demo-app"

//Create a resource group to organize the services related to your container app //deployment.
az group create --name $RESOURCE_GROUP --location $LOCATION --output none

//Create a user-assigned managed identity and get its ID with the following //commands.
//First, create the managed identity.
az identity create --name $IDENTITY_NAME --resource-group $RESOURCE_GROUP --output none

//Now set the identity identifier into a variable for later use.
IDENTITY_ID=$(az identity show \
  --name $IDENTITY_NAME \
  --resource-group $RESOURCE_GROUP \
  --query id \
  --output tsv)

//Create a Container Apps environment to host your app using the following //command.
az containerapp env create --name $ENVIRONMENT --resource-group $RESOURCE_GROUP --location $LOCATION --mi-user-assigned $IDENTITY_ID --output none

//Create an Azure Container Registry (ACR) instance in your resource group. The //registry stores your container image.
az acr create --resource-group $RESOURCE_GROUP --name $REGISTRY_NAME --sku Basic --output none

//Assign your user-assigned managed identity to your container registry instance //with the following command.
az acr identity assign --identities $IDENTITY_ID --name $REGISTRY_NAME --resource-group $RESOURCE_GROUP --output none

#Build and push the image to a registry

//Build and push your container image to your container registry instance with //the following command.
//This command applies the tag helloworld to your container image.
az acr build -t $REGISTRY_NAME".azurecr.io/"$CONTAINER_APP_NAME":helloworld" -r $REGISTRY_NAME .

#Create your container app

//Create your container app with the following command.

//This command adds the acrPull role to your user-assigned managed identity, so //it can pull images from your container registry
az containerapp create --name $CONTAINER_APP_NAME --resource-group $RESOURCE_GROUP --environment $ENVIRONMENT --image $REGISTRY_NAME".azurecr.io/"$CONTAINER_APP_NAME":helloworld" --target-port 8080 --ingress external --user-assigned $IDENTITY_ID --registry-identity $IDENTITY_ID --registry-server $REGISTRY_NAME.azurecr.io --query properties.configuration.ingress.fqdn

Parameter 	Value 	Description
name 	$CONTAINER_APP_NAME 	The name of your container app.
resource-group 	$RESOURCE_GROUP 	The resource group in which your container app is deployed.
environment 	$ENVIRONMENT 	The environment in which your container app runs.
image 	$REGISTRY_NAME".azurecr.io/"$CONTAINER_APP_NAME":helloworld" 	The container image to deploy, including the registry name and tag.
target-port 	8080 	Matches the port that your app is listening to for requests.
ingress 	external 	Makes your container app accessible from the public internet.
user-assigned 	$IDENTITY_ID 	The user-assigned managed identity for your container app.
registry-identity 	registry-identity 	The identity used to access the container registry.
registry-server 	$REGISTRY_NAME.azurecr.io 	The server address of your container registry.
query 	properties.configuration.ingress.fqdn 	Filters the output to just the app's fully qualified domain name (FQDN).
//Once this command completes, it returns URL for your new web app.

#Verify deployment
//Copy the app's URL into a web browser. Once the container app is started, it returns Hello World!.

#Clean up resources
az group delete --name aca-demo


#Next steps
//make changes to your code and update your app in Azure.
https://learn.microsoft.com/en-us/azure/container-apps/tutorial-update-from-code