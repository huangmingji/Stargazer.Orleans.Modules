using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stargazer.Orleans.Template.EntityFrameworkCore.Repositories;

namespace Stargazer.Orleans.Template.EntityFrameworkCore;

public static class EntityFramworkCoreExtensions
{
    public static IServiceCollection UseEntityFramworkCore(this IServiceCollection serviceCollection)
    {
        IConfiguration? configuration = serviceCollection.BuildServiceProvider().GetService<IConfiguration>();
        serviceCollection.AddScoped<IDbContextProvider<EfDbContext>, DbContextProvider<EfDbContext>>();
        Console.WriteLine("11");
        serviceCollection.AddDbContextFactory<EfDbContext>(options =>
        {
            options.UseNpgsql(configuration?.GetConnectionString("Default"));
        }, ServiceLifetime.Scoped);
        Console.WriteLine("22");
        serviceCollection.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        Console.WriteLine("33");
        
        // var assemblies = System.Reflection.Assembly.GetAssembly(typeof(EfDbContext)) ?? throw new NullReferenceException();
        // List<string> types = [typeof(EfCoreRepository<,>).Name];
        // foreach (var definedType in assemblies.DefinedTypes.Where(x => x is {IsClass: true, BaseType: not null} && types.Contains(x.BaseType.Name)))
        // {
        //     var targetInterface = definedType.ImplementedInterfaces.LastOrDefault();
        //     if (targetInterface == null) continue;
        //     serviceCollection.AddScoped(targetInterface, definedType);
        // }
        
        return serviceCollection;
    }
}