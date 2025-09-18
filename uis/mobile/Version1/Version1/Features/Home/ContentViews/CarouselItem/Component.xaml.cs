namespace Version1.Features.Home.ContentViews.CarouselItem;

public partial class Component : ContentView
{
    public static readonly BindableProperty ContextProperty =
        BindableProperty.Create(nameof(Context), typeof(Model), typeof(Component));

    public Model Context
    {
        get => (Model)GetValue(ContextProperty);
        set => SetValue(ContextProperty, value);
    }

    int maxSets = 0;

    #region [ CTors ]

    public Component()
    {
        InitializeComponent();

        this.BindingContext = Context;

    }
    #endregion

    private void ContentView_Loaded(object sender, EventArgs e)
    {

        if (this.Context is not null)
            maxSets = this.Context.Set;
    }

    private void NextButton_Clicked(object sender, EventArgs e)
    {
        if (this.Context.Set >= maxSets)
            return;

        this.Context.Set++;
    }

    private void PreviousButton_Clicked(object sender, EventArgs e)
    {
        if (this.Context.Set == 1)
            return;

        this.Context.Set--;
    }

    private void SubtractButton_Clicked(object sender, EventArgs e)
    {
        if (this.Context.Reps == 0)
            return;

        this.Context.Reps--;
    }

    private void AddButton_Clicked(object sender, EventArgs e)
    {
        this.Context.Reps++;
    }
}