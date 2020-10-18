# azure-dns-docker
This application will discover your current public IP address and then update an A-record of your choice within an Azure DNS Zone.

## Preparing the Azure DNS Zone and Service Principal (one-time setup)
### Ensure that you've created the DNS Zone for the domain in question

Create a DNS Zone using the Azure Portal following [these instructions](https://docs.microsoft.com/en-us/azure/dns/dns-getstarted-portal).

### Create a service principal in Azure Active Directory and create a secret

1. Inside the Azure Portal, find your **Azure Active Directory** page in the left-hand menu.
2. Choose **App Registrations** from the left-hand menu for your Azure Active Directory.
3. Click the **New Registration** button.
4. Provide a name - e.g. AzureDnsDocker - and leave all other values as their defaults.
5. Click **Register**.
6. Copy the **Application (Client) ID** value; you'll need this for the `CLIENT_ID` environment variable.
7. Copy the **Directory (Tenant) ID** value; you'll need this for the `TENANT_ID` environment variable.
8. Click the **Certificates & secrets** option in your app's left-hand menu.
9. Click the **New Client Secret** button.
10. Give the secret a name - e.g. AzureDnsDocker - and choose an expiration period you're comfortable with.
11. Copy the new secret value; you'll need this for the `SECRET` environment variable.
12. Go back to the main Azure left-hand menu (top left) and click **Home**.
13. Under the **Navigate** heading, choose **Subscriptions**.
14. Find the subscription associated with the DNS Zone and copy the **Subscription ID**; you'll need this for the `SUBSCRIPTION_ID` environment variable.

### Grant the service principal read and write access to your DNS Zone

1. Go to your DNS Zone within the Azure Portal.
2. In the zone's left-hand menu, choose **Access control (IAM)**.
3. Click the **Add** button and choose **Add role assignment**.
4. Choose the **DNS Zone Contributor** role.
5. In the **Select** box, start typing the name you chose for your app in the previous section, select the item in the results, and then click **Save**.

## Example Usage

```
docker run -d --name azure-dns-docker \
-e TENANT_ID={tenant-id-guid} \
-e CLIENT_ID={client-id-guid} \
-e SECRET={secret} \
-e SUBSCRIPTION_ID={subscription-id-guid} \
-e RESOURCE_GROUP={resource-group-name} \
-e ZONE_NAME={domain-name}
-e RECORD_NAME={host-name}
cpwood/azure-dns-docker
```

For example, to set the A-record for `foo.bar.com` in the `bar.com` DNS Zone within a Resource Group `Bar`, you'd do this:

```
docker run -d --name azure-dns-docker \
-e TENANT_ID=004097ec-5f22-400d-87ef-9c5a75c8a95c \
-e CLIENT_ID=03022824-f618-430e-8e73-18cef20d35ab \
-e SECRET=oaaodgioadgadoigjda_g8sg7 \
-e SUBSCRIPTION_ID=0a32c718-edf0-443c-bba3-37578e0346c6 \
-e RESOURCE_GROUP=Bar \
-e ZONE_NAME=bar.com
-e RECORD_NAME=foo
cpwood/azure-dns-docker
```

You can also specify the following two optional environment variables:

* `TTL` - the Time-To-Live value for the created A-record (default value is `300` - 300 seconds, i.e. 5 minutes); and
* `DELAY` - the delay between each attempt to check the public IP address and update the DNS Zone (default value is `90000` - 90,000 milliseconds, i.e. 15 minutes).

## Can I update more than one A-record?

No, however you could manually create CNAME records within your DNS Zone pointing to the A-record updated by this application.