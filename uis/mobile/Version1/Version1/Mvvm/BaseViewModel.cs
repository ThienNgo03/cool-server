using Navigation;
using Version1;

namespace Mvvm;

public abstract partial class BaseViewModel : ObservableRecipient
{
    protected App? MyApp { get; }
    protected IAppNavigator AppNavigator { get; }

    protected BaseViewModel(IAppNavigator appNavigator)
    {
        MyApp = Application.Current as App;
        AppNavigator = appNavigator;
    }

    // ReSharper disable once UnusedParameter.Global
    // ReSharper disable once UnusedMethodReturnValue.Global
    // ReSharper disable once VirtualMemberNeverOverridden.Global
    public virtual Task OnAppearingAsync()
    {
        System.Diagnostics.Debug.WriteLine($"{GetType().Name}.{nameof(OnAppearingAsync)}");

        return Task.CompletedTask;
    }

    // ReSharper disable once UnusedParameter.Global
    // ReSharper disable once UnusedMethodReturnValue.Global
    // ReSharper disable once VirtualMemberNeverOverridden.Global
    public virtual Task OnDisappearingAsync()
    {
        System.Diagnostics.Debug.WriteLine($"{GetType().Name}.{nameof(OnDisappearingAsync)}");

        return Task.CompletedTask;
    }

    [RelayCommand]
    protected virtual Task BackAsync() => AppNavigator.GoBackAsync(data: this.GetType().FullName);
}