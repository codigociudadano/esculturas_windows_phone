Imports System.Net
Partial Public Class SplashScreen
    Inherits PhoneApplicationPage

    Public Sub New()
        InitializeComponent()
    End Sub

    Async Sub onLoaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(2.5))
        NavigationService.Navigate(New Uri("/Main.xaml", UriKind.RelativeOrAbsolute))
    End Sub


End Class
