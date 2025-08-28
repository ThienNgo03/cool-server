
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Version1.Features.WebSocket.SignalR;

public partial class Component : ContentView
{

    #region [ Properties ]

    public HubConnection Hub { get; set; } = default!;
    #endregion

    #region [ CTors ]

    public Component()
    {
        InitializeComponent();
    }

    #endregion

    #region [ Bindable Properties ]

    public string BaseUrl
    {
        get => (string)GetValue(BaseUrlProperty);
        set => SetValue(BaseUrlProperty, value);
    }

    public static readonly BindableProperty BaseUrlProperty =
                BindableProperty.Create("BaseUrl", typeof(string), typeof(Component), string.Empty);

    public string EndPoint
    {
        get => (string)GetValue(EndPointProperty);
        set => SetValue(EndPointProperty, value);
    }

    public static readonly BindableProperty EndPointProperty =
                BindableProperty.Create("EndPoint", typeof(string), typeof(Component), string.Empty);

    public string ClientCredential
    {
        get => (string)GetValue(ClientCredentialProperty);
        set => SetValue(ClientCredentialProperty, value);
    }
    public static readonly BindableProperty ClientCredentialProperty =
                BindableProperty.Create("ClientCredential", typeof(string), typeof(Component), string.Empty);

    public bool IsAutoReconnect
    {
        get => (bool)GetValue(IsAutoReconnectProperty);
        set => SetValue(IsAutoReconnectProperty, value);
    }
    public static readonly BindableProperty IsAutoReconnectProperty =
                BindableProperty.Create("IsAutoReconnect", typeof(bool), typeof(Component), true);

    public List<string> Events
    {
        get => (List<string>)GetValue(EventsProperty);
        set => SetValue(EventsProperty, value);
    }
    public static readonly BindableProperty EventsProperty =
                BindableProperty.Create("Events", typeof(List<string>), typeof(Component), null);

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly BindableProperty CommandProperty =
                BindableProperty.Create("Command", typeof(ICommand), typeof(Component), null);
    #endregion

    private async void ContentView_Loaded(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(BaseUrl) || string.IsNullOrEmpty(EndPoint))
        {
            //ShowInfoBar("Error", "BaseUrl and EndPoint must be set before connecting.", InfoBarSeverity.Error);
            return;
        }

        var hubBuilder = new HubConnectionBuilder()
            .WithUrl($"{BaseUrl}/{EndPoint}?userId={ClientCredential}")
            .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
                //logging.AddConsole();
            });

        if (IsAutoReconnect)
            hubBuilder.WithAutomaticReconnect();

        Hub = hubBuilder.Build();
        foreach (var eventName in Events)
        {
            Hub.On<string>(eventName, (id) =>
            {
                var payload = new Payload
                {
                    Event = eventName,
                    Id = id
                };

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (Command is null)
                        return;

                    if (!Command.CanExecute(payload))
                        return;

                    Command.Execute(payload);
                });
            });
        }

        try
        {
            await Hub.StartAsync();
            //ShowInfoBar("Connected", $"Listening for: {string.Join(", ", Events)}", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            //ShowInfoBar("Connection Failed", $"Unable to connect to Hub.\n{ex.Message}", InfoBarSeverity.Error);
        }


    }

    private async void ContentView_Unloaded(object sender, EventArgs e)
    {

        if (Hub != null)
        {
            try
            {
                await Hub.StopAsync();
                await Hub.DisposeAsync();
                Hub = null;
            }
            catch (Exception ex)
            {
                //ShowInfoBar("Disconnect Error", ex.Message, InfoBarSeverity.Error);
            }
        }
    }
}