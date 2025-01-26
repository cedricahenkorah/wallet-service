# WalletService

## Overview

The WalletService is designed to manage user wallets for the Hubtel app. It allows users to securely add, retrieve, and remove wallets. It supports both mobile money (momo) and card wallets, with additional business rules for limiting the number of wallets and preventing duplicates. The service uses MongoDB for storage and .NET 8 for backend development.

Documentation for the API can be found [here](https://documenter.getpostman.com/view/9097411/2sAYQgfngD)

## Tools

- .NET 8
- NoSQL (Mongodb)
- Postman Documentation [here](https://documenter.getpostman.com/view/9097411/2sAYQgfngD)

## Current Implementation

The following features have been implemented:

1. **Wallet API**: RESTful API for managing wallets, supporting operations like adding, deleting, and listing wallets.
2. **Business Logic**: Ensures no more than 5 wallets per user, prevents duplicate wallet additions and only first 6 digits of a card number should be stored.
3. **Authentication**: JWT authentication is used to secure endpoints and ensure only authorized users can access wallet data.
4. **Tests**: Unit tests for the Wallet API and business logic have been implemented using xUnit and Moq.

## Project Structure

1. **Wallet API**

A web service built using ASP.NET Core that exposes endpoints for wallet management and communicates with MongoDB for data persistence.

### Key Features:

- Add a wallet (with business rules to prevent duplicates and limit wallet count).
- Delete a wallet.
- Retrieve a specific wallet by ID.
- List all wallets for a user.
- List all wallets in the system.
- Register a new user.
- Authenticate users with JWT.

### Directory Overview

```shell
src/WalletService
├── Controllers           // API controllers for handling HTTP requests
├── Models                // Entity models (Wallet, User)
├── Enums                 // Enumeration types
├── Services              // Business logic for processing API requests
├── DTOs                  // Data Transfer Objects for structured input/output
├── Repositories          // Data access interfaces and implementations
├── Configurations        // Configuration files for JWT, MongoDB, etc.
├── Program.cs            // Entry point for the API
└── appsettings.json      // Configuration settings (e.g., MongoDB, JWT)
```

2. **WalletService.Tests**

A test project that contains unit tests for the Wallet API and business logic.

## Workflow

1. Add a Wallet
   -User sends a request to add a wallet (either momo or card).
   -The wallet's account number is truncated to the first 6 digits if it's a card wallet.
   -Business rules ensure no duplicates and a maximum of 5 wallets per user.
   -The wallet is saved in the MongoDB database.

2. Retrieve a Wallet
   -User sends a request to retrieve a wallet by ID.
   -The service checks for authorization and fetches the wallet from MongoDB.

3. Delete a Wallet
   -User sends a request to delete a wallet by ID.
   -The service deletes the wallet from MongoDB and confirms the operation.

4. List All Wallets
   -User sends a request to list all wallets associated with their account.
   -The service returns a list of wallet details.

## Project set up.

1. **Clone the repository**

```shell
git clone git@github.com:cedricahenkorah/wallet-service.git
```

2. **Navigate to the project directory**

```shell
cd wallet-service/WalletService.API
```

3. **Install dependencies**

```shell
dotnet restore
```

4. **Build the project**

```shell
dotnet build
```

5. **Run the API**

```shell
dotnet run
```

6. **Run the tests**

```shell
cd ../WalletService.Tests
dotnet test
```

7. **Access the API**

The API will be running on `http://localhost:5150` by default or the PORT you have specified. You can access the API using Postman or any other HTTP client.

## API Documentation

The API documentation can be found [here](https://documenter.getpostman.com/view/9097411/2sAYQgfngD)
