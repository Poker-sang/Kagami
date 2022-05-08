using System.Windows;
using System.Windows.Input;

namespace Kagami.HelpDisplay;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    private void MainWindow_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) => Close();
}