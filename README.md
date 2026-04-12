# deveel.results

A simple, unambitious library to implement the Result Pattern in .NET

## Overview

**deveel.results** is a lightweight and straightforward implementation of the Result Pattern for .NET applications. It provides a structured way to handle operation outcomes, including success and failure scenarios, without relying on exceptions for control flow.

### Key Features

- **Result Pattern Implementation**: Encapsulate operation outcomes (success, failure, unchanged state) in strongly-typed result objects
- **Error Handling**: Rich error modeling with support for error codes, domains, messages, and nested errors
- **Validation Support**: Built-in handling for validation errors with detailed member-level error information
- **ASP.NET Core Integration**: Seamless conversion of operation results to standardized ProblemDetails responses for web APIs
- **Zero Dependencies**: Minimal external dependencies (only `System.ComponentModel.Annotations`)
- **Multi-Target Support**: Available for `.NET Standard 2.0` and `.NET 8.0+`

### Libraries Included

1. **Deveel.Results** - Core library providing the Result Pattern implementation
   - `OperationResult` - Result type for operations that don't return a value
   - `OperationResult<T>` - Generic result type for operations that return a value
   - `OperationError` - Error representation with code, domain, and message
   - Error handling for standard errors and validation errors
   - Extension methods for result manipulation and conversion

2. **Deveel.Results.AspNetCore.Mvc** - ASP.NET Core integration
   - Convert operation results to ASP.NET Core ProblemDetails responses
   - Standardized HTTP error responses for web APIs
   - Support for validation problem details

## Installation

### Via NuGet Package Manager

```bash
Install-Package Deveel.Results
```

For ASP.NET Core integration:

```bash
Install-Package Deveel.Results.AspNetCore.Mvc
```

### Via dotnet CLI

```bash
dotnet add package Deveel.Results
```

For ASP.NET Core integration:

```bash
dotnet add package Deveel.Results.AspNetCore.Mvc
```

### Supported Frameworks

- **Deveel.Results**: `.NET Standard 2.0` and higher
- **Deveel.Results.AspNetCore.Mvc**: `.NET 8.0`, `.NET 9.0`, `.NET 10.0`

## When to Use deveel.results

This library is ideal for the following scenarios:

### 1. **Web API Development**
When building REST or GraphQL APIs, you need consistent error handling and HTTP response formatting. This library provides seamless integration with ASP.NET Core's `ProblemDetails` standard, ensuring your API clients receive standardized error responses with appropriate HTTP status codes.

**Benefits:**
- Automatic conversion of business errors to HTTP problem details
- Consistent error format across all endpoints
- Support for validation errors with field-level details
- Built-in HTTP status code mapping

### 2. **Multi-Layered Application Architecture**
In applications with multiple layers (Controllers → Services → Repositories), error handling often becomes complex. Using the Result Pattern allows errors to propagate cleanly through layers without exception-throwing, making error flows explicit and testable.

**Benefits:**
- Errors are first-class values, not exceptional cases
- Easy to trace error propagation through layers
- Better control over which errors are recoverable
- Cleaner separation of concerns

### 3. **Domain-Driven Design (DDD) Applications**
In DDD, business operations often have multiple possible outcomes beyond success/failure. Using operation results with domain-specific error codes allows you to express business rules and domain constraints clearly.

**Benefits:**
- Express domain logic outcomes explicitly
- Error codes represent business rules
- Domain organization supports bounded contexts
- Validation errors are part of the business domain

### 4. **Service-Oriented Architecture**
When building services that communicate with each other, explicit result handling makes it easier to handle different service response scenarios. Instead of throwing exceptions across service boundaries, results can be propagated cleanly.

**Benefits:**
- Cleaner service-to-service communication
- Easy to implement retry logic based on result types
- Error context is preserved across boundaries
- Simpler to test service interactions

### 5. **Validation-Heavy Operations**
Applications with complex validation requirements (e.g., user registration, order processing) benefit from having validation errors as first-class results rather than exception collections.

**Benefits:**
- Collect all validation errors at once
- Report field-level validation errors
- Distinguish between validation failures and system errors
- Provide detailed feedback to API clients

### 6. **Async Operations and Background Jobs**
When handling long-running operations, background jobs, or event handlers, you need to distinguish between recoverable and non-recoverable failures without relying on exception handling.

**Benefits:**
- Explicit handling of operation outcomes
- Easy to implement retry strategies
- Clean logging of operation failures
- Better observability and monitoring

### 7. **Cross-Cutting Concerns**
Building middleware or filters that need to handle service errors consistently becomes easier with explicit result types rather than trying to catch specific exceptions.

**Benefits:**
- Consistent error handling across the application
- Easy to implement custom error handling middleware
- Better integration with logging frameworks
- Simplified exception handling policies

## Usage Scenarios

### Basic Result Handling

#### Operation with No Return Value

```csharp
using Deveel;

public class UserService
{
    public OperationResult DeleteUser(int userId)
    {
        if (userId <= 0)
            return OperationResult.Fail("INVALID_ID", "User", "User ID must be greater than zero");
        
        try
        {
            // Delete user logic
            return OperationResult.Success;
        }
        catch (Exception ex)
        {
            return OperationResult.Fail("DELETE_FAILED", "User", $"Failed to delete user: {ex.Message}");
        }
    }
}
```

#### Operation with Return Value

```csharp
using Deveel;

public class UserService
{
    public OperationResult<User> GetUser(int userId)
    {
        if (userId <= 0)
            return OperationResult<User>.Fail("INVALID_ID", "User", "User ID must be greater than zero");
        
        var user = _userRepository.FindById(userId);
        if (user == null)
            return OperationResult<User>.Fail("NOT_FOUND", "User", $"User with ID {userId} not found");
        
        return OperationResult<User>.Success(user);
    }
}
```

### Handling Validation Errors

```csharp
using Deveel;
using System.ComponentModel.DataAnnotations;

public class UserService
{
    public OperationResult<User> CreateUser(CreateUserRequest request)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(request);
        
        if (!Validator.TryValidateObject(request, context, validationResults, validateAllProperties: true))
        {
            return OperationResult<User>.ValidationFailed("VALIDATION_FAILED", "User", validationResults);
        }
        
        // Create user logic
        var user = new User { Name = request.Name, Email = request.Email };
        return OperationResult<User>.Success(user);
    }
}
```

### Checking Operation Results

```csharp
public class OrderController
{
    private readonly OrderService _orderService;
    
    public void ProcessOrder(int orderId)
    {
        var result = _orderService.GetOrder(orderId);
        
        if (result.IsSuccess())
        {
            var order = result.Value;
            Console.WriteLine($"Order found: {order.Id}");
        }
        else if (result.IsError())
        {
            Console.WriteLine($"Error: {result.Error?.Code} - {result.Error?.Message}");
        }
        else if (result.IsUnchanged())
        {
            Console.WriteLine("Order was not changed");
        }
    }
}
```

### ASP.NET Core API Integration

```csharp
using Deveel;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    
    [HttpPost]
    public IResult CreateUser([FromBody] CreateUserRequest request)
    {
        var result = _userService.CreateUser(request);
        
        if (result.IsSuccess())
            return Results.Created($"/api/users/{result.Value?.Id}", result.Value);
        
        if (result.IsError())
            return result.AsProblem(HttpContext, statusCode: 400);
        
        return Results.BadRequest();
    }
    
    [HttpGet("{id}")]
    public IResult GetUser(int id)
    {
        var result = _userService.GetUser(id);
        
        if (result.IsSuccess())
            return Results.Ok(result.Value);
        
        if (result.IsError())
        {
            var statusCode = result.Error?.Code == "NOT_FOUND" ? 404 : 400;
            return result.AsProblem(HttpContext, statusCode: statusCode);
        }
        
        return Results.BadRequest();
    }
    
    [HttpDelete("{id}")]
    public IResult DeleteUser(int id)
    {
        var result = _userService.DeleteUser(id);
        
        if (result.IsSuccess())
            return Results.NoContent();
        
        if (result.IsError())
            return result.AsProblem(HttpContext, statusCode: 400);
        
        return Results.BadRequest();
    }
}
```

### Error Domain Organization

Organize your errors by domain for better error categorization:

```csharp
public class AuthService
{
    public OperationResult<AuthToken> Authenticate(string username, string password)
    {
        if (string.IsNullOrEmpty(username))
            return OperationResult<AuthToken>.Fail("INVALID_USERNAME", "Auth", "Username cannot be empty");
        
        var user = _userRepository.FindByUsername(username);
        if (user == null)
            return OperationResult<AuthToken>.Fail("USER_NOT_FOUND", "Auth", "User not found");
        
        if (!_passwordHasher.Verify(password, user.PasswordHash))
            return OperationResult<AuthToken>.Fail("INVALID_PASSWORD", "Auth", "Invalid password");
        
        var token = _tokenService.GenerateToken(user);
        return OperationResult<AuthToken>.Success(token);
    }
}
```

### Chaining Operations

```csharp
public class OrderService
{
    public OperationResult<Order> ProcessOrder(OrderRequest request)
    {
        // Validate order
        var validationResult = ValidateOrder(request);
        if (validationResult.IsError())
            return validationResult; // Implicitly convert OperationResult to OperationResult<Order>
        
        // Create order
        var order = new Order { /* ... */ };
        var createResult = _orderRepository.Save(order);
        if (createResult.IsError())
            return OperationResult<Order>.Fail(createResult.Error!);
        
        // Process payment
        var paymentResult = _paymentService.ProcessPayment(order.Id, request.Amount);
        if (paymentResult.IsError())
            return OperationResult<Order>.Fail(paymentResult.Error!);
        
        return OperationResult<Order>.Success(order);
    }
}
```

### Result Type Checks

```csharp
var result = _userService.GetUser(userId);

// Check for specific conditions
if (result.ResultType == OperationResultType.Success)
{
    // Handle successful result
}
else if (result.ResultType == OperationResultType.Error)
{
    // Handle error result
}
else if (result.ResultType == OperationResultType.Unchanged)
{
    // Handle unchanged state
}
```

## Real-World Examples

### Example 1: E-Commerce Order Processing

This example demonstrates a complete order processing workflow using the Result Pattern with multiple validation steps, service calls, and error handling:

```csharp
using Deveel;

public class OrderProcessingService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentService _paymentService;
    private readonly IInventoryService _inventoryService;
    private readonly INotificationService _notificationService;

    public OperationResult<Order> ProcessNewOrder(CreateOrderRequest request)
    {
        // Step 1: Validate the order request
        if (request == null || request.Items.Count == 0)
            return OperationResult<Order>.Fail(
                "INVALID_ORDER", "Order", "Order must contain at least one item");

        // Step 2: Check inventory availability
        var inventoryCheck = _inventoryService.CheckAvailability(request.Items);
        if (inventoryCheck.IsError())
            return OperationResult<Order>.Fail(inventoryCheck.Error!);

        // Step 3: Create the order
        var order = new Order
        {
            OrderNumber = Guid.NewGuid().ToString(),
            CustomerId = request.CustomerId,
            Items = request.Items,
            TotalAmount = request.Items.Sum(i => i.Price * i.Quantity),
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var savedOrder = _orderRepository.Create(order);
        if (savedOrder == null)
            return OperationResult<Order>.Fail(
                "ORDER_CREATION_FAILED", "Order", "Failed to save order to database");

        // Step 4: Process payment
        var paymentResult = _paymentService.Charge(request.PaymentMethod, savedOrder.TotalAmount);
        if (paymentResult.IsError())
        {
            // Rollback: cancel the order if payment fails
            _orderRepository.Cancel(savedOrder.Id);
            return OperationResult<Order>.Fail(
                "PAYMENT_FAILED", "Payment", $"Payment processing failed: {paymentResult.Error?.Message}");
        }

        // Step 5: Reserve inventory
        var reservationResult = _inventoryService.Reserve(savedOrder.Id, request.Items);
        if (reservationResult.IsError())
        {
            // Rollback: refund payment and cancel order
            _paymentService.Refund(paymentResult.Value?.TransactionId!);
            _orderRepository.Cancel(savedOrder.Id);
            return OperationResult<Order>.Fail(reservationResult.Error!);
        }

        // Step 6: Update order status
        savedOrder.Status = OrderStatus.Confirmed;
        _orderRepository.Update(savedOrder);

        // Step 7: Send confirmation notification (fire and forget)
        _ = _notificationService.SendOrderConfirmation(savedOrder.Id);

        return OperationResult<Order>.Success(savedOrder);
    }

    public OperationResult CancelOrder(string orderId)
    {
        var order = _orderRepository.GetById(orderId);
        if (order == null)
            return OperationResult.Fail("ORDER_NOT_FOUND", "Order", $"Order {orderId} not found");

        if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
            return OperationResult.Fail("CANNOT_CANCEL", "Order", "Cannot cancel shipped or delivered orders");

        // Release reserved inventory
        var releaseResult = _inventoryService.Release(order.Id);
        if (releaseResult.IsError())
            return releaseResult;

        // Process refund if payment was made
        if (order.Status == OrderStatus.Confirmed)
        {
            var refundResult = _paymentService.Refund(order.PaymentTransactionId);
            if (refundResult.IsError())
                return refundResult;
        }

        order.Status = OrderStatus.Cancelled;
        _orderRepository.Update(order);

        _ = _notificationService.SendCancellationNotification(order.Id);
        return OperationResult.Success;
    }
}
```

### Example 2: User Registration with Validation

This example shows how to handle complex user registration with multiple validation steps:

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserRegistrationService _registrationService;
    private readonly ILogger<AuthController> _logger;

    [HttpPost("register")]
    public async Task<IResult> Register([FromBody] RegisterUserRequest request)
    {
        _logger.LogInformation("User registration attempt for email: {Email}", request.Email);

        // Call the service
        var result = await _registrationService.RegisterUserAsync(request);

        // Handle different outcomes
        if (result.IsSuccess())
        {
            _logger.LogInformation("User registered successfully: {UserId}", result.Value?.Id);
            return Results.Created($"/api/auth/{result.Value?.Id}", new { result.Value?.Id });
        }

        if (result.IsError())
        {
            var error = result.Error!;
            var statusCode = error.Code switch
            {
                "EMAIL_ALREADY_EXISTS" => 409, // Conflict
                "WEAK_PASSWORD" => 400,         // Bad Request
                "INVALID_EMAIL" => 400,         // Bad Request
                _ => 500                        // Internal Server Error
            };

            _logger.LogWarning("Registration failed: {ErrorCode} - {ErrorMessage}", error.Code, error.Message);
            return result.AsProblem(HttpContext, statusCode: statusCode, title: "Registration Failed");
        }

        return Results.BadRequest();
    }
}

public class UserRegistrationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IEmailService _emailService;

    public async Task<OperationResult<User>> RegisterUserAsync(RegisterUserRequest request)
    {
        // Step 1: Validate input
        if (string.IsNullOrWhiteSpace(request.Email))
            return OperationResult<User>.Fail("INVALID_EMAIL", "Auth", "Email is required");

        if (string.IsNullOrWhiteSpace(request.Password))
            return OperationResult<User>.Fail("INVALID_PASSWORD", "Auth", "Password is required");

        if (request.Password.Length < 8)
            return OperationResult<User>.Fail(
                "WEAK_PASSWORD", "Auth", "Password must be at least 8 characters long");

        // Step 2: Check if email already exists
        var existingUser = await _userRepository.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return OperationResult<User>.Fail(
                "EMAIL_ALREADY_EXISTS", "Auth", $"Email {request.Email} is already registered");

        // Step 3: Hash password
        var passwordHash = _passwordService.HashPassword(request.Password);

        // Step 4: Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FullName = request.FullName,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            IsEmailVerified = false
        };

        // Step 5: Save user
        var savedUser = await _userRepository.CreateAsync(user);
        if (savedUser == null)
            return OperationResult<User>.Fail(
                "REGISTRATION_FAILED", "Auth", "Failed to create user account");

        // Step 6: Send verification email (fire and forget)
        _ = _emailService.SendVerificationEmailAsync(savedUser.Id, savedUser.Email);

        return OperationResult<User>.Success(savedUser);
    }
}
```

### Example 3: Data Import with Partial Success

This example demonstrates handling scenarios where some operations succeed and others fail:

```csharp
public class DataImportService
{
    public OperationResult<ImportResult> ImportUsers(IEnumerable<UserImportDto> users)
    {
        var importResult = new ImportResult
        {
            TotalProcessed = 0,
            SuccessfulImports = 0,
            FailedImports = new List<ImportError>()
        };

        foreach (var userDto in users)
        {
            importResult.TotalProcessed++;

            // Validate user data
            if (string.IsNullOrWhiteSpace(userDto.Email))
            {
                importResult.FailedImports.Add(new ImportError
                {
                    RowNumber = importResult.TotalProcessed,
                    Email = userDto.Email,
                    ErrorCode = "INVALID_EMAIL",
                    ErrorMessage = "Email is required"
                });
                continue;
            }

            // Check for duplicates
            if (_userRepository.ExistsByEmail(userDto.Email))
            {
                importResult.FailedImports.Add(new ImportError
                {
                    RowNumber = importResult.TotalProcessed,
                    Email = userDto.Email,
                    ErrorCode = "DUPLICATE_EMAIL",
                    ErrorMessage = "User with this email already exists"
                });
                continue;
            }

            // Create user
            var createResult = CreateUserFromDto(userDto);
            if (createResult.IsError())
            {
                importResult.FailedImports.Add(new ImportError
                {
                    RowNumber = importResult.TotalProcessed,
                    Email = userDto.Email,
                    ErrorCode = createResult.Error?.Code,
                    ErrorMessage = createResult.Error?.Message
                });
                continue;
            }

            // Save user
            var savedUser = _userRepository.Create(createResult.Value!);
            if (savedUser != null)
                importResult.SuccessfulImports++;
            else
            {
                importResult.FailedImports.Add(new ImportError
                {
                    RowNumber = importResult.TotalProcessed,
                    Email = userDto.Email,
                    ErrorCode = "SAVE_FAILED",
                    ErrorMessage = "Failed to save user to database"
                });
            }
        }

        return OperationResult<ImportResult>.Success(importResult);
    }
}
```

## Extension Methods

The library provides useful extension methods for working with results:

- `IsSuccess()` - Check if operation succeeded
- `IsError()` - Check if operation failed
- `IsUnchanged()` - Check if operation resulted in no changes
- `AsException()` - Convert error to an exception
- `AsProblem()` - Convert result to ASP.NET Core ProblemDetails (AspNetCore.Mvc package)

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
