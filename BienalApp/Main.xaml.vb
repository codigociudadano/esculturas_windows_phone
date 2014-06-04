Partial Public Class PanoramaPage1
    Inherits PhoneApplicationPage

    Public Sub New()
        InitializeComponent()
    End Sub
    Private Sub Autores(sender As Object, e As RoutedEventArgs)
        NavigationService.Navigate(New Uri("/Autores.xaml", UriKind.RelativeOrAbsolute))
    End Sub

    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
        MyBase.OnNavigatedTo(e)
        If NavigationContext.QueryString.ContainsKey("fromIdObra") Then
            NavigationService.RemoveBackEntry()
            NavigationService.RemoveBackEntry()
        End If
    End Sub

    Sub onLoaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        NavigationService.RemoveBackEntry()
    End Sub

    Private Sub Esculturas(sender As Object, e As RoutedEventArgs)
        NavigationService.Navigate(New Uri("/ListaEsculturas.xaml", UriKind.RelativeOrAbsolute))
    End Sub

    Private Sub idObra(sender As Object, e As RoutedEventArgs) Handles btnIdObra.Click
        NavigationService.Navigate(New Uri("/idObra.xaml", UriKind.RelativeOrAbsolute))
    End Sub
End Class
