Imports System.Net.Http
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.Windows.Media.Imaging
Imports System.IO.IsolatedStorage


Partial Public Class Page1
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
        setPG(True)
        LoadEscultura(nid)
    End Sub

    Sub setPG(val As Boolean)
        SystemTray.ProgressIndicator.IsVisible = val
    End Sub

    Private Async Sub LoadEscultura(nid As String)
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
        'Acá viene la faena de las fotos, agarrate catalina e.e
        Dim storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication
        For Each token As JToken In array.Item("field_fotos").Item("und")
            Dim uriFull As String = token.Item("uri").ToString.Replace("public://", App.baseurl + "/sites/default/files/")
            Dim nombreFoto As String = uriFull.Split("/")(uriFull.Split("/").Count - 1)
            Dim image As Image = New Image
            Dim bmimage As BitmapImage
            If storage.FileExists(nombreFoto) Then
                'Cargar imagen desde la sd
                bmimage = New BitmapImage
                Dim stream As IO.Stream = Nothing
                Try
                    stream = storage.OpenFile(nombreFoto, IO.FileMode.Open, IO.FileAccess.Read)
                Catch ex As IO.FileNotFoundException
                    MessageBox.Show("No se encontró el archivo, muchacho")
                    Exit Sub
                Catch ex As Exception
                    storage.DeleteFile(nombreFoto)
                End Try
                If stream.Length = 0 Then
                    storage.DeleteFile(nombreFoto)
                Else
                    bmimage.SetSource(stream)
                End If
            Else
                'Bajar desde internet para cargar desde la sd :P
                Try
                    response = Await http.GetAsync(uriFull)
                    response.EnsureSuccessStatusCode()
                Catch hre As HttpRequestException
                    MessageBox.Show("Algo anduvo mal con el request :/")
                    setPG(False)
                    Exit Sub
                End Try
                Dim streamBM = Await response.Content.ReadAsStreamAsync
                bmimage = New BitmapImage
                bmimage.SetSource(streamBM)
                Dim stream As IsolatedStorageFileStream = storage.CreateFile(nombreFoto)
                Dim wb As WriteableBitmap = New WriteableBitmap(bmimage)
                wb.SaveJpeg(stream, bmimage.PixelWidth, bmimage.PixelHeight, 0, 100)
                stream.Close()
            End If
            image.Source = bmimage
            image.Width = 400
            image.HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            spFotos.Children.Add(image)
        Next
        Dim desc As String = array.Item("body").Item("und").Item(0).Item("value").ToString.Trim
        tbDesc.Text = desc
        Dim idAutor As String = array.Item("field_autor").Item("und").Item(0).Item("target_id").ToString
        Try
            response = Await http.GetAsync(App.baseurl + "/api/v1/node/" + idAutor)
            response.EnsureSuccessStatusCode()
        Catch hre As HttpRequestException
            MessageBox.Show("Algo anduvo mal con el request :/")
            setPG(False)
            Exit Sub
        End Try
        content = Await response.Content.ReadAsStringAsync
        Dim arrayAutor As JObject = JObject.Parse(content)
        Dim autor As String = arrayAutor.Item("title").ToString
        tbAutor.Text = autor
        tbElab.Text = "Año de elaboración: " + array.Item("field_ano").Item("und").Item(0).Item("value").ToString.Trim
        Dim idEvento As String = array.Item("field_evento").Item("und").Item(0).Item("target_id").ToString
        Try
            response = Await http.GetAsync(App.baseurl + "/api/v1/node/" + idEvento)
            response.EnsureSuccessStatusCode()
        Catch hre As HttpRequestException
            MessageBox.Show("Algo anduvo mal con el request :/")
            setPG(False)
            Exit Sub
        End Try
        content = Await response.Content.ReadAsStringAsync
        Dim arrayEvento As JObject = JObject.Parse(content)
        Dim evento As String = "Se presentó en " + arrayEvento.Item("title").ToString.Trim
        tbEvento.Text = evento
        Dim idTipo As String = array.Item("field_tipo").Item("und").Item(0).Item("tid").ToString.Trim
        Try
            response = Await http.GetAsync(App.baseurl + "/api/v1/taxonomy_term/" + idTipo)
            response.EnsureSuccessStatusCode()
        Catch hre As HttpRequestException
            MessageBox.Show("Algo anduvo mal con el request :/")
            setPG(False)
            Exit Sub
        End Try
        content = Await response.Content.ReadAsStringAsync
        Dim arrayTipo As JObject = JObject.Parse(content)
        Dim tipo As String = "Tipo: " + arrayTipo.Item("name").ToString.Trim
        tbTipo.Text = tipo
        Dim idMaterial As String = array.Item("field_material").Item("und").Item(0).Item("tid").ToString
        Try
            response = Await http.GetAsync(App.baseurl + "/api/v1/taxonomy_term/" + idMaterial)
            response.EnsureSuccessStatusCode()
        Catch hre As HttpRequestException
            MessageBox.Show("Algo anduvo mal con el request :/")
            setPG(False)
            Exit Sub
        End Try
        content = Await response.Content.ReadAsStringAsync
        Dim arrayMaterial As JObject = JObject.Parse(content)
        Dim material As String = "Material: " + arrayMaterial.Item("name").ToString.Trim
        tbMaterial.Text = material
        'Los premios, con un for each re loco
        For Each token As JToken In array.Item("field_premios").Item("und")
            Dim tb As New TextBlock
            tb.Text = token.Item("value")
            Dim left = 12, top = 0, right = 0, bottom = 0
            tb.Margin = New Thickness(left, top, right, bottom)
            tb.TextWrapping = TextWrapping.Wrap
            spPremios.Children.Add(tb)
        Next
        setPG(False)
    End Sub
End Class
