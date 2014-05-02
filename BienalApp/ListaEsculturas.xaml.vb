Imports System.Windows.Media.Imaging
Imports System.Net.Http
Imports Newtonsoft.Json.Linq


Partial Public Class ListaEsculturas
    Inherits PhoneApplicationPage

    Partial Public Class esculturaElement
        Public Property Title As String
        Public Property ImageSource As BitmapImage
        Public Property nid As String
    End Class

    Dim listaEsculturas As List(Of esculturaElement)
    Dim cargado As Boolean

    Public Sub New()
        InitializeComponent()
        listaEsculturas = New List(Of esculturaElement)
        cargado = False
    End Sub

    Sub setPG(val As Boolean)
        SystemTray.ProgressIndicator.IsVisible = val
    End Sub

    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
        MyBase.OnNavigatedTo(e)
        If cargado = False Then
            SystemTray.IsVisible = True
            SystemTray.ProgressIndicator = New ProgressIndicator()
            SystemTray.ProgressIndicator.IsIndeterminate = True
            SystemTray.ProgressIndicator.Text = "Trayendo esculturas..."
            setPG(True)
            LoadEsculturas()
        End If
        llEsculturas.SelectedItem = Nothing

    End Sub

    Private Async Sub LoadEsculturas()
        Dim http As New HttpClient
        Dim response As Http.HttpResponseMessage
        Dim content As String
        Try
            response = Await http.GetAsync(App.baseurl + "/api/v1/node?parameters[type]=escultura")
            response.EnsureSuccessStatusCode()
        Catch hre As HttpRequestException
            MessageBox.Show("Algo anduvo mal con el request :/")
            Exit Sub
        End Try
        content = Await response.Content.ReadAsStringAsync
        Dim array As JArray = JArray.Parse(content)
        For Each token As JToken In array
            Dim tempEscultura As esculturaElement = New esculturaElement
            tempEscultura.nid = token.Item("nid").ToString
            tempEscultura.Title = token.Item("title").ToString
            Try
                response = Await http.GetAsync(token.Item("uri").ToString)
                response.EnsureSuccessStatusCode()
            Catch hre As HttpRequestException
                MessageBox.Show("Algo anduvo mal con el request :/")
                Exit Sub
            End Try
            content = Await response.Content.ReadAsStringAsync
            Dim jEscultura As JObject = JObject.Parse(content)
            Dim urlFoto As String = jEscultura.Item("field_fotos").Item("und").Item(0).Item("uri").ToString.Replace("public://", App.baseurl + "/sites/default/files/")
            tempEscultura.ImageSource = New BitmapImage(New Uri(urlFoto, UriKind.Absolute))
            listaEsculturas.Add(tempEscultura)
        Next
        llEsculturas.ItemsSource = listaEsculturas
        cargado = True
        setPG(False)
    End Sub

    Private Sub verEscultura(ByVal sender As Object, ByVal e As EventArgs) Handles llEsculturas.SelectionChanged
        If llEsculturas.SelectedItem IsNot Nothing Then
            Dim item As esculturaElement = llEsculturas.SelectedItem
            NavigationService.Navigate(New Uri("/Escultura.xaml?nid=" + item.nid.Trim, UriKind.RelativeOrAbsolute))
        End If
    End Sub
End Class
