var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello World!");
app.MapPost("/", () => "This is a POST request");
app.MapDelete("/", () => "This is a DELETE request!");
app.MapPut("/", () => "This is a PUT request!");
app.MapPatch("/", () => "This is a PATCH request!");

app.Run();
