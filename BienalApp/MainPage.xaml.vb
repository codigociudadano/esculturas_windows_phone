Imports System
Imports System.Threading
Imports System.Windows.Controls
Imports Microsoft.Phone.Controls
Imports Microsoft.Phone.Shell
Imports System.Net.Http
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.Windows
Imports System.Windows.Navigation

Partial Public Class MainPage
    Inherits PhoneApplicationPage
     ' Constructor
    Public Sub New()
        InitializeComponent()

        SupportedOrientations = SupportedPageOrientation.Portrait

        ' Código de ejemplo para traducir ApplicationBar
        'BuildLocalizedApplicationBar()

    End Sub

    ' Código de ejemplo para compilar una ApplicationBar traducida
    'Private Sub BuildLocalizedApplicationBar()
    '    ' Establecer ApplicationBar de la página en una nueva instancia de ApplicationBar.
    '    ApplicationBar = New ApplicationBar()

    '    ' Crear un nuevo botón y establecer el valor de texto en la cadena traducida de AppResources.
    '    Dim appBarButton As New ApplicationBarIconButton(New Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative))
    '    appBarButton.Text = AppResources.AppBarButtonText
    '    ApplicationBar.Buttons.Add(appBarButton)

    '    ' Crear un nuevo elemento de menú con la cadena traducida de AppResources.
    '    Dim appBarMenuItem As New ApplicationBarMenuItem(AppResources.AppBarMenuItemText)
    '    ApplicationBar.MenuItems.Add(appBarMenuItem)
    'End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        NavigationService.Navigate(New Uri("/Escultura.xaml?nid=605", UriKind.RelativeOrAbsolute))
    End Sub

    Private Sub navigateToQR(sender As Object, e As RoutedEventArgs)
        NavigationService.Navigate(New Uri("/Autores.xaml", UriKind.RelativeOrAbsolute))
    End Sub

    
    Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
        NavigationService.Navigate(New Uri("/ListaEsculturas.xaml", UriKind.RelativeOrAbsolute))
    End Sub
End Class