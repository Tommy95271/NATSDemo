using Microsoft.Extensions.DependencyInjection;
using NATS.Client;

namespace JetStreamShared.Extensions
{
    /// <summary>
    /// Provides extension methods for NATS registration with ASP.Net Core DI
    /// </summary>
    public static class NatsServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a NATS connection singleton to the services collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
        /// <param name="options">An optional <see cref="Action{T}" /> to configure the provided <see cref="Options" />.</param>
        /// <returns>A reference to the <see cref="IServiceCollection" /> after the operation has completed.</returns>
        public static IServiceCollection AddNats(this IServiceCollection services, Action<Options> options = null)
        {
            var opts = ConnectionFactory.GetDefaultOptions();
            options?.Invoke(opts);

            return services.AddSingleton<IConnection>(_ => new ConnectionFactory().CreateConnection(opts));
        }

        /// <summary>
        /// Adds a scoped NATS connection to the services collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
        /// <param name="options">An optional <see cref="Action{T}" /> to configure the provided <see cref="Options" />.</param>
        /// <returns>A reference to the <see cref="IServiceCollection" /> after the operation has completed.</returns>
        public static IServiceCollection AddNatsScoped(this IServiceCollection services, Action<Options> options = null)
        {
            return services.AddScoped<IConnection>(_ =>
            {
                var opts = ConnectionFactory.GetDefaultOptions();
                options?.Invoke(opts);
                return new ConnectionFactory().CreateConnection(opts);
            });
        }

        /// <summary>
        /// Adds a transient NATS connection to the services collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the service to.</param>
        /// <param name="options">An optional <see cref="Action{T}" /> to configure the provided <see cref="Options" />.</param>
        /// <returns>A reference to the <see cref="IServiceCollection" /> after the operation has completed.</returns>
        public static IServiceCollection AddNatsTransient(this IServiceCollection services, Action<Options> options = null)
        {
            return services.AddTransient<IConnection>(_ =>
            {
                var opts = ConnectionFactory.GetDefaultOptions();
                options?.Invoke(opts);
                return new ConnectionFactory().CreateConnection(opts);
            });
        }
    }
}
