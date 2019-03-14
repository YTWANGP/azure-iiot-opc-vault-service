# UA Xamarin Client
This sample runs on the following platform: Windows (UWP).

The client has implemented these functionalities:
- Connect to azure-iiot-opc-vault-service to get registered OPC UA server application list and CSR request list
- Connect to an OPC UA server with administrator to create CSR and send new request to azure-iiot-opc-vault-service.
- Dowload new certificate and issuer from azure-iiot-opc-vault-service and push to OPC UA server.

## How to build and run the sample in Visual Studio on Windows

### Prerequisites:
Install Windows 10 Fall Creators Update.

Install latest Visual Studio 2017 (min version 15.5.5).

Add [Xamarin](https://developer.xamarin.com/guides/cross-platform/getting_started/installation/windows/#vs2017) to Visual Studio.

Create clientSecret for this xamarin app to connect to azure-iiot-opc-vault-service
- Go to Azure portal -> Choose Azure AD Directory -> App Registration -> find OPC Vault module -> Choose Settings -> Choose Keys -> Newly Add Password for Xamarin app -> save the key value for the xamarin using.


### UWP:
1. Modify Settings.cs with OPC Vault module information.
2. Select UA Xamarin Client.UWP as startup project.
3. Hit `F5` to build and execute the sample.

