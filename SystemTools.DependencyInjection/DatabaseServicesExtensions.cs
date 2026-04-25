using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SystemTools.SystemToolsShared;

namespace SystemTools.DependencyInjection;

public static class DatabaseServicesExtensions
{
    public static IServiceCollection AddContextByProvider<TContext>(this IServiceCollection services,
        EDatabaseProvider? dataProvider, string connectionString, int commandTimeout) where TContext : DbContext
    {
        switch (dataProvider)
        {
            case EDatabaseProvider.SqlServer:
                services.AddDbContext<TContext>(options => options.UseSqlServer(connectionString, sqlOptions =>
                {
                    if (commandTimeout > -1)
                    {
                        sqlOptions.CommandTimeout(commandTimeout);
                    }
                }));
                break;
            case EDatabaseProvider.None:
            case EDatabaseProvider.SqLite:
            case EDatabaseProvider.OleDb:
            case EDatabaseProvider.WebAgent:
            case null:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataProvider));
        }

        return services;
    }
}
