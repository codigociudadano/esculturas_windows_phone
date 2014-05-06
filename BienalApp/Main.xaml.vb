Partial Public Class PanoramaPage1
    Inherits PhoneApplicationPage

    Public Sub New()
        InitializeComponent()
    End Sub
    Private Sub Autores(sender As Object, e As RoutedEventArgs)
        NavigationService.Navigate(New Uri("/Autores.xaml", UriKind.RelativeOrAbsolute))
    End Sub


    Private Sub Esculturas(sender As Object, e As RoutedEventArgs)
        NavigationService.Navigate(New Uri("/ListaEsculturas.xaml", UriKind.RelativeOrAbsolute))
    End Sub
End Class
