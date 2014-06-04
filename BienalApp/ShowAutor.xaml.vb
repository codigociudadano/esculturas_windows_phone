Imports System.Net.Http
Imports System.Windows.Media.Imaging
Imports Newtonsoft.Json.Linq
Imports System.IO.IsolatedStorage
Imports System.Text.RegularExpressions

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
        SystemTray.ProgressIndicator.Text = "Cargando autor..."
        setpg(True)
        loadAutor(nid)
    End Sub

    Private Sub setpg(val As Boolean)
        If SystemTray.ProgressIndicator IsNot Nothing Then
            SystemTray.ProgressIndicator.IsVisible = val
        End If
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
        Dim title As String = array.Item("title").ToString.Trim
        tbTitle.Text = title
        Dim desc As String = array.Item("body").Item("und").Item(0).Item("value").ToString.Trim
        desc = desc.Replace("<br/>", Environment.NewLine)
        desc = desc.Replace("<br />", Environment.NewLine)
        desc = desc.Replace("<br>", Environment.NewLine)
        Dim descNoHtml = Regex.Replace(desc, "<[^>]+>|&nbsp;", "").Trim
        tbDesc.Text = descNoHtml
        Dim storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication
        If array.Item("field_fotos").HasValues Then
            Dim urifull As String = array.Item("field_fotos").Item("und").Item(0).Item("uri").ToString.Replace("public://", App.baseurl + "/sites/default/files/")
            Dim nombreFoto As String = urifull.Split("/")(urifull.Split("/").Count - 1)
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
                    response = Await http.GetAsync(urifull)
                    response.EnsureSuccessStatusCode()
                Catch hre As HttpRequestException
                    MessageBox.Show("Algo anduvo mal con el request :/")
                    setpg(False)
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
            spFotos.Children.Add(image)
        End If
        setpg(False)
    End Sub


End Class
