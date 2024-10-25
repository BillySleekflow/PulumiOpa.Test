# Pulumi Opa Test 
Before running this project, you need to install and prepare the following tools:
- [Pulumi](https://www.pulumi.com/docs/get-started/install/)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)

You should create a new Pulumi account and Azure account before running this project.

## Step 1: Login to Azure CLI
```bash
az login
```

## Step 2: Create a new Pulumi project
```bash
pulumi new pulumiOpa
```

```console
(base) kahongchan@BillyChansMBP14 Pulumi Opa Test 1 % az login
A web browser has been opened at https://login.microsoftonline.com/organizations/oauth2/v2.0/authorize. Please continue the login in the web browser. If no web browser is available or if the web browser fails to open, use device code flow with `az login --use-device-code`.

Retrieving tenants and subscriptions for the selection...

[Tenant and subscription selection]

No     Subscription name         Subscription ID                       Tenant
-----  ------------------------  ------------------------------------  --------
[1] *  Azure Basic subscription  dc26b866-d66d-4087-82cf-17a179593cda  - 

The default is marked with an *; the default tenant is '- ' and subscription is 'Azure Basic subscription' (dc26b866-d66d-4087-82cf-17a179593cda).

Select a subscription and tenant (Type a number or Enter for no changes): 1

Tenant: - 
Subscription: Azure Basic subscription (dc26b866-d66d-4087-82cf-17a179593cda)

[Announcements]
With the new Azure CLI login experience, you can select the subscription you want to use more easily. Learn more about it and its configuration at https://go.microsoft.com/fwlink/?linkid=2271236

If you encounter any problem, please open an issue at https://aka.ms/azclibug

[Warning] The login output has been updated. Please be aware that it no longer displays the full list of available subscriptions by default.

(base) kahongchan@BillyChansMBP14 PulumiOpaTest % pulumi stack       
Current stack is dev:
    Owner: BillySleekflow
Current stack resources (0):
    No resources currently in this stack

More information at: https://app.pulumi.com/BillySleekflow/PulumiOpaTest/dev

Use `pulumi stack select` to change stack; `pulumi stack ls` lists known ones
(base) kahongchan@BillyChansMBP14 PulumiOpaTest % pulumi stack ls
NAME  LAST UPDATE  RESOURCE COUNT  URL
dev*  n/a          n/a             https://app.pulumi.com/BillySleekflow/PulumiOpaTest/dev
(base) kahongchan@BillyChansMBP14 PulumiOpaTest % pulumi stack select BillySleekflow/PulumiOpaTest/dev
(base) kahongchan@BillyChansMBP14 PulumiOpaTest % pulumi preview
Previewing update (dev)

View in Browser (Ctrl+O): https://app.pulumi.com/BillySleekflow/PulumiOpaTest/dev/previews/b88a3742-0ff5-4cc8-a5a2-b51096126cb1



error: failed to decrypt configuration key 'auth0:client_secret': [400] Bad Request: invalid ciphertext
(base) kahongchan@BillyChansMBP14 PulumiOpaTest % pulumi config set auth0:client_id ******* --secret
(base) kahongchan@BillyChansMBP14 PulumiOpaTest % pulumi config set auth0:client_secret ******* --secret 
(base) kahongchan@BillyChansMBP14 PulumiOpaTest % dotnet build
(base) kahongchan@BillyChansMBP14 PulumiOpaTest % pulumi preview
Previewing update (dev)

View in Browser (Ctrl+O): https://app.pulumi.com/BillySleekflow/PulumiOpaTest/dev/previews/e0d0280a-fa66-46b7-9c99-a77866a63af1

     Type                                                 Name                                      Plan       
 +   pulumi:pulumi:Stack                                  PulumiOpaTest-dev                         create     
 +   ├─ azure-native:insights:Component                   sleekflow-container-apps-env-app-insight  create     
 +   └─ azure-native:resources:ResourceGroup              sleekflow-resource-group-dev              create     
 +      ├─ azure-native:app/v20240301:ManagedEnvironment  sleekflow-container-apps-env              create     
 +      ├─ azure-native:operationalinsights:Workspace     sleekflow                                 create     
 +      └─ azure-native:containerregistry:Registry        myregistry                                create     

Resources:
    + 6 to create

(base) kahongchan@BillyChansMBP14 PulumiOpaTest % 

```