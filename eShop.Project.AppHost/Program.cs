var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Ordering_API>("ordering-api");

builder.Build().Run();
