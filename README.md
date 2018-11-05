
# Sample Gateway App

This is a sample application intended to serve as a model for any developer wanting to build an integration for a redirect payment gateway with 3dcart.
The app is built in .NET MCV (C#) and it is structured as follows:

## 3dcartSampleGatewayApp

**Web.config**

-- 3 settings need to be properly configured: AppTestPublicKey, AppTestSecretKey and BaseURLWebAPIService

-- 1 connection string: connStr

**Controllers**

-- AuthenticationController: Manages the installation of the app in any 3dcart store

-- CheckoutController: Manages the checkout processed between the cart and the gateway

-- HomeController: Reders the installation button that initiates the oauth process (refer to AuthenticationController)

-- RefundController: Manages the post-order transactions (refunds/voids) between the cart and the gateway

**Models**

-- Cart: classes representing the data exchanged with the cart

-- Gateway: classes representing the data exchanged with the payment gateway

**Services**

-- Business logic implementation

-- Methods helping exchanging data between the controllers and the external endpoints (cart and gateway)

**Infrastructure**

-- Repository: Methods used to persist and retrieve data

-- WebAPIClient: Helper web client methods


## 3dcartSampleGatewayAppTest

Unit tests covering most of the methods described above - using NUnit and mocking the necessary data
