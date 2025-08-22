using Microsoft.EntityFrameworkCore;
using WebAPI;
using WebAPI.Data;
using WebAPI.Middleware;
using WebAPI.Utils;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAllServices(builder.Configuration);

var app = builder.Build();

EmailSender.Initialize(app.Configuration);
JwtHandler.Initialize(app.Configuration);

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
}
app.UseMiddleware<GlobalExceptionMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Skibidi Hopita API V1");
        c.RoutePrefix = "swagger";
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        c.DefaultModelsExpandDepth(0);
    });
}
app.UseHttpsRedirection();
app.UseCors();
app.UseMiddleware<JWTAuthenticationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ResponseWrappingMiddleware>();
app.MapControllers();

app.Run();
