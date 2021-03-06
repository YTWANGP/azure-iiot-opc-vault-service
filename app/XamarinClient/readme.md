# OPC Vault Certificate Management Client
This sample runs on the following platform: Windows (UWP).

The client has implemented these functionalities:

- Connect to azure-iiot-opc-vault-service to get registered OPC UA server application list and CSR request list

- Connect to an OPC UA server with administrator to create CSR and send new request to azure-iiot-opc-vault-service.

- Download new certificate and issuer from azure-iiot-opc-vault-service and push to OPC UA server.

## Install and run Xamarin UWP package directly
We provide sample app package for you to install and run the application. There are x86 and x64 versions under XamarinClient.UWP\AppPackages directory.

1. Install dependent package under XamarinClient.UWP\AppPackages\Dependencies directory.
2. Manually sign the Digital Signature
    - Right Click UWP package -> Properties -> Digital Signatures -> Details -> View Certificate -> Install Certificate
    - Choose local machine as Store Location -> Choose Place all certificates in the following store and browse to "Trusted People"
3. Install UWP package
4. Import AAD configuration at the first time running. You can have deploy\xamarin.appsettings.json with AAD configuration after deploy OPC Vault service with deploy.ps1 or create the configuration file by yourself.

## How to build and run the sample in Visual Studio on Windows

### Prerequisites:
1. Install Windows 10 Fall Creators Update.

2. Install latest Visual Studio 2017 (min version 15.5.5).

3. Add [Xamarin](https://developer.xamarin.com/guides/cross-platform/getting_started/installation/windows/#vs2017) to Visual Studio.

4. Create clientSecret for this xamarin app to connect to azure-iiot-opc-vault-service

    - Go to Azure portal -> Choose Azure AD Directory -> App Registration -> find OPC Vault module -> Choose Settings -> Choose Keys -> Newly Add Password for Xamarin app -> save the key value for the xamarin app using.
    
    ![image](https://github.com/YTWANGP/azure-iiot-opc-vault-service/blob/master/docs/ClientSecrect.png)

### UWP:
1. Modify xamarin.appsettings.json with OPC Vault module and tenant information.
   - TenantId : your tenant id
   - AppServiceURL as OPC Vault service url, ex: https://opcvault-service.azurewebsites.net
   - clientId : opcvault-module application id
   - graphResourceUri : opcvault-service applicaiton id
   - ClientSecret : opcvault-module keys
   
2. Select UA Xamarin Client.UWP as startup project.

3. Hit `F5` to build and execute the sample.

4. Import xamarin.appsettings.json at first time running.

## How to use the sample xamarin app

1. Create new CSR of OPC UA server
    - click menu on the left top of app ->  click 'Application' to get the registered application list.
    
    ![image](https://github.com/YTWANGP/azure-iiot-opc-vault-service/blob/master/docs/Applicationlist.png)
    - choose registered application you want to create new request and get appliction details -> click 'New Request' button.
    
    ![image](https://github.com/YTWANGP/azure-iiot-opc-vault-service/blob/master/docs/ApplicationDetails.png)
    - Connect to the registered appliction with administrator username/password.
    
    ![image](https://github.com/YTWANGP/azure-iiot-opc-vault-service/blob/master/docs/ConnectServer.png)
    - Get current Certificate group and click 'CreatCSR'.
    
    ![image](https://github.com/YTWANGP/azure-iiot-opc-vault-service/blob/master/docs/CertificateGroupBefore.png)
    - Get CertificateRequest as Base64 encoded and click 'Generate New Certificate'.
    
    ![image](https://github.com/YTWANGP/azure-iiot-opc-vault-service/blob/master/docs/GenerateNewCert.png)
    - After CertificateRequest is created, xamarin app will navigate to 'Certificate Request' page and list CSR in 'New', 'Approved', 'Rejected' states. You can keep checking this page and when CSR is approved, you can push new certificate to server.
    
    ![image](https://github.com/YTWANGP/azure-iiot-opc-vault-service/blob/master/docs/CSRList.png)
 
2. Push new certificate to OPC UA server
    - Choose approved CSR and get certificate request details with new certificate, issuer and crl as Based64 encoded.
    
    ![image](https://github.com/YTWANGP/azure-iiot-opc-vault-service/blob/master/docs/CSRDetails.png)
    - Click 'Download Certificate to server' button -> connect to OPC UA server with administrator username/password -> get current certificate group -> click 'Download Certificate to server' button
    
    ![image](https://github.com/YTWANGP/azure-iiot-opc-vault-service/blob/master/docs/DownloadCert.png)
    
    - After download completely, you can check certificate group with new certificate
    
    ![image](https://github.com/YTWANGP/azure-iiot-opc-vault-service/blob/master/docs/CertificateGroupAfter.png)
