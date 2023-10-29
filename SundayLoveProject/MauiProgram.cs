using Syncfusion.Maui.Core.Hosting;
using System.Reflection;

namespace SundayLoveProject;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
        var assembly = typeof(App).GetTypeInfo().Assembly;
        var assemblyName = assembly.GetName().Name;
		var filename = $"Resources.syncfusion";
        using (var stream = assembly.GetManifestResourceStream($"{assemblyName}.{filename}")) {
            if (stream != null) {
				using (StreamReader reader = new StreamReader(stream)) {
					var syncLis = reader.ReadToEnd();
					Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncLis);
                }
			}
        }
		filename = $"Resources.firebase";
        using (var stream = assembly.GetManifestResourceStream($"{assemblyName}.{filename}")) {
            if (stream != null) {
                using (StreamReader reader = new StreamReader(stream)) {
                    FirebaseUtility.fbName = reader.ReadLine();
					FirebaseUtility.fbID = reader.ReadLine();
                }
            }
        }

        builder.ConfigureSyncfusionCore();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		return builder.Build();
	}
}
