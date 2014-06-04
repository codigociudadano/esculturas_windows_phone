Imports System.Windows.Media.Imaging
Imports System.Net.Http
Imports Newtonsoft.Json.Linq
Imports WP8Classes
Imports System.IO.IsolatedStorage



Partial Public Class ListaEsculturas
    Inherits PhoneApplicationPage

    Partial Public Class esculturaElement
        Public Property Title As String
        Public Property ImageSource As BitmapImage
        Public Property nid As String
    End Class

    Dim lastRefreshed As DateTime
    Dim listaEsculturas As List(Of esculturaElement)
    Dim cargado As Boolean
    Dim currentPage As Integer
    Dim page As Integer

    Public Sub New()
        InitializeComponent()
        listaEsculturas = New List(Of esculturaElement)
        cargado = False
        page = 0
    End Sub

    Sub setPG(val As Boolean)
        If SystemTray.ProgressIndicator IsNot Nothing Then
            SystemTray.ProgressIndicator.IsVisible = val
        End If
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
            response = Await http.GetAsync(App.baseurl + "/api/v1/esculturas?pagesize=5&page=" + page.ToString)
            response.EnsureSuccessStatusCode()
        Catch hre As HttpRequestException
            MessageBox.Show("Algo anduvo mal con el request :/")
            Exit Sub
        End Try
        Dim storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication
        Dim lastitem As esculturaElement = Nothing
        If llEsculturas.ItemsSource IsNot Nothing Then
            lastitem = llEsculturas.ItemsSource(llEsculturas.ItemsSource.Count - 1)
        End If
        content = Await response.Content.ReadAsStringAsync
        Dim json As JToken = JToken.Parse(content)
        Dim array As JArray = json.Item("data")
        For Each token As JToken In array
            Dim tempEscultura As esculturaElement = New esculturaElement
            tempEscultura.nid = token.Item("id").ToString
            tempEscultura.Title = token.Item("name").ToString.Trim
            Dim urlFoto As String = token.Item("image").ToString
            Dim nombreFoto As String = urlFoto.Split("/")(urlFoto.Split("/").Count - 1)
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
                response = Await http.GetAsync(urlFoto)
                Dim streamBM = Await response.Content.ReadAsStreamAsync
                bmimage = New BitmapImage
                bmimage.SetSource(streamBM)
                Dim stream As IsolatedStorageFileStream = storage.CreateFile(nombreFoto)
                Dim wb As WriteableBitmap = New WriteableBitmap(bmimage)
                wb.SaveJpeg(stream, bmimage.PixelWidth, bmimage.PixelHeight, 0, 100)
                stream.Close()
            End If
            tempEscultura.ImageSource = bmimage
            listaEsculturas.Add(tempEscultura)
        Next
        llEsculturas.ItemsSource = Nothing
        llEsculturas.ItemsSource = listaEsculturas
        If lastitem IsNot Nothing Then
            llEsculturas.ScrollTo(lastitem)
        End If
        If page = 0 Then
            Dim pullDetector As New WP8PullDetector
            pullDetector.Bind(llEsculturas)
            AddHandler pullDetector.Compression, AddressOf onCompression
        End If
        lastRefreshed = DateTime.Now
        page += 1
        cargado = True
        setPG(False)
    End Sub

    Private Sub verEscultura(ByVal sender As Object, ByVal e As EventArgs) Handles llEsculturas.SelectionChanged
        If llEsculturas.SelectedItem IsNot Nothing Then
            Dim item As esculturaElement = llEsculturas.SelectedItem
            NavigationService.Navigate(New Uri("/Escultura.xaml?nid=" + item.nid.Trim, UriKind.RelativeOrAbsolute))
        End If
    End Sub

    Sub onCompression(sender As Object, e As CompressionEventArgs)
        If DateTime.Now - lastRefreshed > TimeSpan.FromSeconds(1.5) And e.Type = CompressionType.Bottom Then
            setPG(True)
            LoadEsculturas()
        End If
    End Sub

End Class
