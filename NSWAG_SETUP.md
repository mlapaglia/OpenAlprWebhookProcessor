# NSwag TypeScript Client Generation Setup

This document describes how to use NSwag to automatically generate type-safe TypeScript client code for your Angular frontend from your ASP.NET Core API.

## Overview

NSwag provides automatic code generation from OpenAPI/Swagger specifications, giving you:

- **Type Safety**: Fully typed API calls with IntelliSense support
- **Auto-sync**: Keep frontend and backend in sync automatically
- **Error Handling**: Built-in error handling and HTTP client configuration
- **Authentication**: JWT token support with automatic header injection

## Setup Components

### 1. Backend Configuration (ASP.NET Core)

The backend has been configured with NSwag middleware:

```csharp
// In Startup.cs - ConfigureServices
services.AddOpenApiDocument(config =>
{
    config.PostProcess = document =>
    {
        document.Info = new OpenApiInfo
        {
            Version = "v1",
            Title = "OpenALPR Webhook Processor API",
            Description = "API for managing license plate recognition webhook processing"
        };
    };
    
    // JWT Authentication support
    config.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
    {
        Type = OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = OpenApiSecurityApiKeyLocation.Header,
        Description = "Type into the textbox: Bearer {your JWT token}."
    });
});

// In Startup.cs - Configure
app.UseOpenApi(); // Serves the OpenAPI spec at /swagger/v1/swagger.json
app.UseSwaggerUi(); // Serves the Swagger UI at /swagger
```

### 2. Frontend Configuration

The Angular frontend includes:
- **NSwag CLI**: For generating TypeScript clients
- **Configuration**: `nswag.config.json` for generation settings
- **Services**: Wrapper service for the generated client
- **Build Scripts**: Automated generation during development

## Usage Instructions

### Step 1: Start the Backend API

```bash
cd OpenAlprWebhookProcessor.Server
dotnet run
```

The API will be available at `https://localhost:5001` and the Swagger UI at `https://localhost:5001/swagger`

### Step 2: Generate the TypeScript Client

```bash
cd openalprwebhookprocessor.client

# Install dependencies (if not already done)
npm install

# Generate the TypeScript client
npm run generate-client
```

This will:
1. Check if the API is running
2. Download the OpenAPI specification
3. Generate `src/app/_generated/api-client.ts`
4. Create all necessary TypeScript interfaces and client classes

### Step 3: Update Your Services

After generation, update your services to use the generated client:

```typescript
// src/app/_services/api.service.ts
import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiClient, API_BASE_URL } from '../_generated/api-client';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly apiClient: ApiClient;

  constructor(
    private http: HttpClient,
    @Inject(API_BASE_URL) baseUrl: string
  ) {
    this.apiClient = new ApiClient(http, baseUrl);
  }

  // Type-safe API calls with IntelliSense
  getUserById(id: number): Observable<User> {
    return this.apiClient.getUserById(id);
  }

  getAlerts(): Observable<Alert[]> {
    return this.apiClient.getAlerts();
  }

  createAlert(alert: Alert): Observable<void> {
    return this.apiClient.addAlert(alert);
  }
}
```

## Generated Code Structure

The generated `api-client.ts` includes:

### 1. TypeScript Interfaces
```typescript
export interface Alert {
    id?: string;
    plateNumber: string;
    description: string;
    isStrictMatch: boolean;
}

export interface User {
    id: number;
    username: string;
    firstName: string;
    lastName: string;
}
```

### 2. API Client Classes
```typescript
@Injectable()
export class ApiClient {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl !== undefined && baseUrl !== null ? baseUrl : "";
    }

    getAlerts(): Observable<Alert[]> {
        // Generated implementation with proper error handling
    }

    addAlert(body: Alert): Observable<void> {
        // Generated implementation with proper validation
    }
}
```

### 3. Error Handling
```typescript
export class ApiException extends Error {
    message: string;
    status: number;
    response: string;
    headers: { [key: string]: any; };
    result: any;

    constructor(message: string, status: number, response: string, headers: { [key: string]: any; }, result: any) {
        super();
        // Error handling implementation
    }
}
```

## Build Integration

### Development Workflow

```bash
# Start development with automatic client generation
npm run dev:with-client

# Or generate client separately when API changes
npm run generate-client
npm start
```

### Production Build

```bash
# Build with fresh client generation
npm run build:with-client
```

### Automated Integration

Add to your CI/CD pipeline:

```yaml
# GitHub Actions example
- name: Generate API Client
  run: |
    cd OpenAlprWebhookProcessor.Server
    dotnet run --urls="https://localhost:5001" &
    sleep 10
    cd ../openalprwebhookprocessor.client
    npm run generate-client
    npm run build
```

## Configuration Options

### NSwag Configuration (`nswag.config.json`)

Key settings you can customize:

```json
{
  "codeGenerators": {
    "openApiToTypeScriptClient": {
      "className": "ApiClient",
      "template": "Angular",
      "rxJsVersion": 7.0,
      "httpClass": "HttpClient",
      "operationGenerationMode": "MultipleClientsFromOperationId",
      "generateClientInterfaces": false,
      "generateOptionalParameters": false,
      "wrapDtoExceptions": false,
      "useTransformOptionsMethod": false,
      "output": "src/app/_generated/api-client.ts"
    }
  }
}
```

### Common Customizations

1. **Multiple Clients**: Generate separate clients per controller
   ```json
   "operationGenerationMode": "MultipleClientsFromOperationId"
   ```

2. **Interface Generation**: Generate interfaces for better testability
   ```json
   "generateClientInterfaces": true
   ```

3. **RxJS Version**: Match your Angular RxJS version
   ```json
   "rxJsVersion": 7.0
   ```

## Authentication Integration

The generated client automatically integrates with your JWT interceptor:

```typescript
// The client respects your HTTP interceptors
export class ApiClient {
  // HTTP calls automatically include Authorization headers
  // via your existing JwtInterceptor
}
```

## Troubleshooting

### Common Issues

1. **API Not Running**
   ```
   Error: connect ECONNREFUSED localhost:5001
   ```
   Solution: Start the backend API first

2. **CORS Issues**
   ```
   Access-Control-Allow-Origin error
   ```
   Solution: Ensure CORS is properly configured in Startup.cs

3. **SSL Certificate Issues**
   ```
   DEPTH_ZERO_SELF_SIGNED_CERT
   ```
   Solution: Accept the development certificate or disable SSL validation

### Debugging

1. **Check OpenAPI Spec**: Visit `https://localhost:5001/swagger/v1/swagger.json`
2. **Verify Generation**: Check if `src/app/_generated/api-client.ts` exists
3. **Build Logs**: Run `npm run generate-client:dev` for detailed output

## Benefits

✅ **Type Safety**: Compile-time error checking for API calls
✅ **IntelliSense**: Full autocomplete and documentation
✅ **Sync**: Frontend automatically updated when API changes
✅ **Error Handling**: Consistent error handling across all API calls
✅ **Authentication**: Automatic JWT token handling
✅ **Maintenance**: No manual DTO updates needed

## Next Steps

1. Run `npm run generate-client` to create your first client
2. Update existing HTTP calls to use the generated client
3. Add the generation step to your build pipeline
4. Consider generating multiple clients for different API areas

For more advanced scenarios, see the [NSwag Documentation](https://github.com/RicoSuter/NSwag/wiki) 