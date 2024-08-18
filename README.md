#  Microservice Design Project

# E-commerce Microservice Project Timeline

| Date Range | Phase | Tasks |
|------------|-------|-------|
| Juny 2024 - Aug 2024 | Backend Development | - Basic Structure<br>- Database Setup<br>- Caching & Message Queue<br>- API Endpoints |
| June 2024 - July 2024 | Frontend Development | - Initial Setup<br>- Frontend-Backend Integration<br>- User Interface Development<br>- State Management |
| Aug 2024 - Sep 2024 | Integration & Testing | - Authentication & Authorization<br>- Error Handling & Validation<br>- Testing |
| Jul 2024 - Nov 2024 | Optimization & Deployment | - Performance Optimization<br>- Logging & Monitoring<br>- Security Audit<br>- Documentation<br>- Deployment |

## Current Status (as of July 2024):
- API development is complete.
- Frontend development is in progress, with initial setup done and integration underway.
- Integration & Testing phase is beginning.
- Optimization & Deployment phase is upcoming.

## Next Steps:
1. Complete Frontend-Backend Integration
2. Finish User Interface Development
3. Implement Authentication & Authorization
4. Begin comprehensive testing


## Architecture Diagram

```mermaid
graph TD
    A[React Frontend] -->|HTTP| B[API Gateway]
    B -->|HTTP| C[Command Service]
    B -->|HTTP| D[Query Service]
    C -->|Publish Events| E[RabbitMQ]
    E -->|Consume Events| F[Saga Orchestrator]
    F -->|Publish Commands| E
    C -->|Write| G[(Postgres)]
    D -->|Read| H[(Redis)]
    I[Event Handler] -->|Update| H
    E -->|Consume Events| I
```

## Class Diagram
classDiagram
    class ApplicationDbContext {
        +DbSet~Product~ Products
        +DbSet~User~ Users
        +DbSet~Order~ Orders
        +DbSet~OrderItem~ OrderItems
        +DbSet~ShippingAddress~ ShippingAddresses
        +DbSet~Cart~ Carts
        +DbSet~CartItem~ CartItems
        +DbSet~Review~ Reviews
    }

    class Product {
        +int Id
        +string Name
        +string Description
        +decimal Price
        +int StockQuantity
        +string Category
        +List~string~ Images
        +List~Review~ Reviews
        +bool IsDeleted
    }

    class User {
        +int Id
        +string Email
        +string PasswordHash
        +string FirstName
        +string LastName
        +string Address
        +string PhoneNumber
        +string Role
        +List~Order~ Orders
        +Cart Cart
        +bool IsDeleted
    }

    class Order {
        +int Id
        +int UserId
        +DateTime OrderDate
        +string Status
        +decimal TotalAmount
        +List~OrderItem~ Items
        +ShippingAddress ShippingAddress
        +bool IsDeleted
    }

    class OrderItem {
        +int Id
        +int OrderId
        +int ProductId
        +int Quantity
        +decimal Price
    }

    class ShippingAddress {
        +int Id
        +int OrderId
        +string FullName
        +string AddressLine1
        +string AddressLine2
        +string City
        +string State
        +string PostalCode
        +string Country
    }

    class Cart {
        +int Id
        +int UserId
        +List~CartItem~ Items
    }

    class CartItem {
        +int Id
        +int CartId
        +int ProductId
        +int Quantity
    }

    class Review {
        +int Id
        +int ProductId
        +int UserId
        +int Rating
        +string Comment
        +DateTime CreatedAt
    }

    ApplicationDbContext "1" --> "*" Product : Contains
    ApplicationDbContext "1" --> "*" User : Contains
    ApplicationDbContext "1" --> "*" Order : Contains
    ApplicationDbContext "1" --> "*" OrderItem : Contains
    ApplicationDbContext "1" --> "*" ShippingAddress : Contains
    ApplicationDbContext "1" --> "*" Cart : Contains
    ApplicationDbContext "1" --> "*" CartItem : Contains
    ApplicationDbContext "1" --> "*" Review : Contains

    User "1" --> "*" Order : Places
    User "1" --> "1" Cart : Has
    Order "1" --> "*" OrderItem : Contains
    Order "1" --> "1" ShippingAddress : Has
    Cart "1" --> "*" CartItem : Contains
    Product "1" --> "*" Review : Has
    Product "1" --> "*" OrderItem : In
    Product "1" --> "*" CartItem : In
