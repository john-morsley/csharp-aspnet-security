var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerAndOpenAi();
builder.Services.AddOAuth(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSwaggerAndOpenAi();
app.UseHttpsRedirection();
app.UseCors();
app.UseOAuth();

app.MapControllers();

app.Run();

public partial class Program { }