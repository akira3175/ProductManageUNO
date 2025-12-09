using ProductManageUNO.Services;
using SQLitePCL;

namespace ProductManageUNO;
public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
        SQLitePCL.Batteries_V2.Init();
    }

    protected Window? MainWindow { get; private set; }
    public IHost? Host { get; private set; } // Changed to public

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
            .UseToolkitNavigation()
            .Configure(host => host
#if DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .UseLogging(configure: (context, logBuilder) =>
                {
                    logBuilder
                        .SetMinimumLevel(
                            context.HostingEnvironment.IsDevelopment() ?
                                LogLevel.Information :
                                LogLevel.Warning)
                        .CoreLogLevel(LogLevel.Warning);
                }, enableUnoLogging: true)
                .UseConfiguration(configure: configBuilder =>
                    configBuilder
                        .EmbeddedSource<App>()
                        .Section<AppConfig>()
                )
                .UseLocalization()
                .UseHttp((context, services) =>
                {
#if DEBUG
                    services.AddTransient<DelegatingHandler, DebugHttpHandler>();
#endif
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<IApiService, ApiService>();
                    services.AddHttpClient<ApiService>();
                    services.AddTransient<ICartService, CartService>();
                    services.AddTransient<MainModel>();
                    services.AddTransient<ProductDetailModel>();
                    services.AddSingleton<CartModel>();
                    services.AddDbContext<Data.AppDbContext>();
                })
                .UseNavigation(ReactiveViewModelMappings.ViewModelMappings, RegisterRoutes)
            );

        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();

        // ✅ Build host trước
        Host = await builder.NavigateAsync<Shell>();

        // ✅ SAU ĐÓ mới khởi tạo database
        try
        {
            if (Host?.Services != null)
            {
                using var scope = Host.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();
                await dbContext.Database.EnsureCreatedAsync();
                Console.WriteLine("✅ Database initialized successfully");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Database initialization error: {ex.Message}");
        }
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellModel)),
            new ViewMap<MainPage, MainModel>(),
            new DataViewMap<SecondPage, SecondModel, Entity>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellModel>(),
                Nested:
                [
                    new ("Main", View: views.FindByViewModel<MainModel>(), IsDefault:true),
                    new ("Second", View: views.FindByViewModel<SecondModel>()),
                ]
            )
        );
    }
}
