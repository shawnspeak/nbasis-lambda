# NBasis.Lambda [![NuGet Prerelease Version](https://img.shields.io/nuget/vpre/NBasis.Lambda.svg?style=flat)](https://www.nuget.org/packages/NBasis.Lambda/)

Simplifies and standardizes the construction of AWS Lambdas with dotnet. Additionally the NBasis.Lambda.RuntimeSupport makes docker based lambdas easy.

Features:
- Uses _IServiceProvider_ for IoC throughout the function
- _IConfiguration_ is available with the Environment variables included

### A simple S3 event function

**First**, install the _NBasis.Lambda_ [NuGet package](https://www.nuget.org/packages/NBasis.Lambda) into your app.

```shell
dotnet add package Amazon.Lambda.S3Events
dotnet add package NBasis.Lambda
```

**Next**, add a function class to your project based on _NBasis.Lambda.BaseEventFunction_

```csharp
using Amazon.Lambda.S3Events;
using NBasis.Lambda;

public class S3EventFunction : BaseEventFunction<S3Event, S3EventHandler>
{
    protected override void SetupServices(IServiceCollection services) 
    {
        // add any services needed for your handler
    }
}

```

**Then**, add a handler class which inherits from _NBasis.Lambda.IHandleLambdaEvent_

```csharp
using NBasis.Lambda;

public class S3EventHandler : IHandleLambdaEvent<S3Event>
{
    // services can be passed to the constructor
    public S3EventHandler() 
    {
    }

    public async Task HandleEventAsync(S3Event input, ILambdaContext context)
    {
    }
}

```

**Finally**, update your CloudFormation template (or pulumi, teraform, etc) with the handler path

```json

"S3EventHandler" : {
    "Type" : "AWS::Serverless::Function",
    "Properties": {
        "Handler": "Simple3Event::Simple3Event.S3EventFunction::FunctionHandler",

```

