Imports System.Net.Http
Imports System.Windows.Media.Imaging
Imports Newtonsoft.Json.Linq

Partial Public Class ShowAutor
    Inherits PhoneApplicationPage

    Public Sub New()
        InitializeComponent()
    End Sub

    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
        MyBase.OnNavigatedTo(e)
        Dim nid As String = ""
        NavigationContext.QueryString.TryGetValue("nid", nid)
        SystemTray.IsVisible = True
        SystemTray.ProgressIndicator = New ProgressIndicator()
        SystemTray.ProgressIndicator.IsIndeterminate = True
        SystemTray.ProgressIndicator.Text = "Cargando escultura..."
        setpg(True)
        loadAutor(nid)
    End Sub

    Private Sub setpg(val As Boolean)
        SystemTray.IsVisible = val
    End Sub

    Private Async Sub loadAutor(nid As String)
        Dim http As New HttpClient
        Dim response As HttpResponseMessage
        Dim content As String
        Try
            response = Await http.GetAsync(App.baseurl + "/api/v1/node/" + nid)
            response.EnsureSuccessStatusCode()
        Catch hre As HttpRequestException
            MessageBox.Show("Algo anduvo mal con el request :/")
            Exit Sub
        End Try
        content = Await response.Content.ReadAsStringAsync
        Dim array As JObject = JObject.Parse(content)
        Dim title As String = array.Item("title").ToString
        tbTitle.Text = title
        Dim desc As String = array.Item("body").Item("und").Item(0).Item("value").ToString
        tbDesc.Text = desc
        Dim urlFoto As String = array.Item("field_fotos").Item("und").Item(0).Item("uri").ToString.Replace("public://", App.baseurl + "/sites/default/files/")
        Dim image As New Image
        image.Source = New BitmapImage(New Uri(urlFoto, UriKind.RelativeOrAbsolute))
        spFotos.Children.Add(image)
    End Sub


End Class
