using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using LocationShareApp.ViewModels;

namespace LocationShareApp.Views;

public partial class ModernMapPage : ContentPage
{
    public ModernMapPage(ModernMapViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnMapClicked(object sender, MapClickedEventArgs e)
    {
        if (BindingContext is ModernMapViewModel viewModel)
        {
            await viewModel.AddMarkerCommand.ExecuteAsync(e.Location);
        }
    }

    private void OnCurrentLocationClicked(object sender, EventArgs e)
    {
        if (BindingContext is ModernMapViewModel viewModel)
        {
            viewModel.GoToCurrentLocationCommand.Execute(null);
        }
    }

    private void OnSearchClicked(object sender, EventArgs e)
    {
        if (BindingContext is ModernMapViewModel viewModel)
        {
            viewModel.SearchLocationCommand.Execute(null);
        }
    }

    private void OnLayersClicked(object sender, EventArgs e)
    {
        if (BindingContext is ModernMapViewModel viewModel)
        {
            viewModel.ToggleMapTypeCommand.Execute(null);
        }
    }

    private void OnShareClicked(object sender, EventArgs e)
    {
        if (BindingContext is ModernMapViewModel viewModel)
        {
            viewModel.ShareLocationCommand.Execute(null);
        }
    }
}