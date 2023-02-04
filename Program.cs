var builder = WebApplication.CreateBuilder(args);
// o componenta reutilizabila care ofera functionalitate aplicatiei
// creează canalul de procesare a cererilor din aplicații - cum raspunde aplicatia la http requsts
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    
    app.UseSwagger();
    app.UseSwaggerUI();
}

// extension methods pentru a adauga componente 'middleware' la canalul de cereri
app.UseHttpsRedirection();    // face https redirection de la http

app.UseAuthorization();

app.MapControllers();

app.Run();
