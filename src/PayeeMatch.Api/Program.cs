using Microsoft.Extensions.DependencyInjection.Extensions;
using PayeeMatch.Accounts;
using PayeeMatch.Api.Verification;
using PayeeMatch.Decisions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(new DecisionEngine());

// a bank registers its own directory; without one every check answers NOAP
builder.Services.TryAddSingleton<IAccountDirectory>(new InMemoryAccountDirectory([]));

var app = builder.Build();

// a body that isn't JSON fails before the endpoint runs; this gives it a ProblemDetails body too
app.UseStatusCodePages();

app.MapHealthChecks("/health");
app.MapPayeeVerification();

app.Run();
