﻿namespace Uno.Extensions.Configuration;

public static class HostBuilderExtensions
{
	public static IHostBuilder UseConfiguration(
		this IHostBuilder hostBuilder,
		Action<IConfigurationBuilder>? configureHostConfiguration = default,
		Action<HostBuilderContext, IConfigurationBuilder>? configureAppConfiguration = default,
		Func<IConfigBuilder, IHostBuilder>? configure = default)
	{
		if (configureHostConfiguration is not null)
		{
			hostBuilder = hostBuilder.ConfigureHostConfiguration(configureHostConfiguration);
		}

		if (configureAppConfiguration is not null)
		{
			hostBuilder = hostBuilder.ConfigureAppConfiguration(configureAppConfiguration);
		}

		hostBuilder = hostBuilder.ConfigureServices((ctx, s) =>
				{
					s.TryAddSingleton(a => ctx.Configuration);
					s.TryAddSingleton(a => (IConfigurationRoot)ctx.Configuration);
					s.TryAddSingleton<Reloader>();
					s.TryAddSingleton<ReloadService>();
					_ = s.AddHostedService(sp => sp.GetRequiredService<ReloadService>());
					s.TryAddSingleton<IStartupService>(sp => sp.GetRequiredService<ReloadService>());
				});
		var configBuilder = hostBuilder.AsConfigBuilder();
		hostBuilder = configure?.Invoke(configBuilder) ?? hostBuilder;

		if(configBuilder is ConfigBuilder cBuilder && !cBuilder.UseEmbeddedSourceFiles)
		{
			// Use content source by default
			configBuilder.ContentSource();
		}

		return hostBuilder;
	}

	
}
