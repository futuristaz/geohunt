var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<TimerService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/timer/start", (TimerService timer) => timer.Start());
app.MapGet("/timer/status", (TimerService timer) => timer.Status());
app.MapGet("/timer/reset", (TimerService timer) => timer.Reset());

app.Run();