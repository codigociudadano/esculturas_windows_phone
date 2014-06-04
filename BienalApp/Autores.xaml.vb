Imports System.Net.Http
Imports Newtonsoft.Json.Linq
Imports System.Windows.Media.Imaging
Imports System.IO.IsolatedStorage
Imports WP8Classes



Partial Public Class autorElement
    Public Property Nombre As String
    Public Property Desc As String
    Public Property ImageSource As BitmapImage
    Public Property nid As String
End Class

Partial Public Class Autor
    Inherits PhoneApplicationPage

    Dim lastRefreshed As DateTime
    Dim cargado As Boolean
    Dim currentPage As Integer
    Dim page As Integer
    Dim listaAutores As List(Of autorElement)

    Public Sub New()
        InitializeComponent()
        listaAutores = New List(Of autorElement)
        cargado = False
        page = 0
    End Sub

    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
        MyBase.OnNavigatedTo(e)
        If cargado = False Then
            SystemTray.IsVisible = True
            SystemTray.ProgressIndicator = New ProgressIndicator()
            SystemTray.ProgressIndicator.IsIndeterminate = True
            SystemTray.ProgressIndicator.Text = "Trayendo autores..."
            setPG(True)
            LoadAutores()
        End If
        llAutores.SelectedItem = Nothing
    End Sub

    Sub setPG(val As Boolean)
        If SystemTray.ProgressIndicator IsNot Nothing Then
            SystemTray.ProgressIndicator.IsVisible = val
        End If
    End Sub

    Private Async Sub LoadAutores()
        Dim http As New HttpClient
        Dim response = Await http.GetAsync(App.baseurl + "/api/v1/autores?pagesize=6&page=" + page.ToString)
        Dim content As String = Await response.Content.ReadAsStringAsync
        Dim json As JToken = JToken.Parse(content)
        Dim array As JArray = json.Item("data")
        Dim storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication
        Dim lastitem As autorElement = Nothing
        If llAutores.ItemsSource IsNot Nothing Then
            lastitem = llAutores.ItemsSource(llAutores.ItemsSource.Count - 1)
        End If
        For Each token As JToken In array
            Dim tempautor As autorElement = New autorElement
            tempautor.nid = token.Item("id").ToString
            tempautor.Nombre = token.Item("name").ToString.Trim
            Dim urlFoto As String = token.Item("image").ToString.Trim
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
            tempautor.ImageSource = bmimage
            listaAutores.Add(tempautor)
        Next
        llAutores.ItemsSource = Nothing
        llAutores.ItemsSource = listaAutores
        If lastitem IsNot Nothing Then
            llAutores.ScrollTo(lastitem)
        End If
        If page = 0 Then
            Dim pullDetector As New WP8PullDetector
            pullDetector.Bind(llAutores)
            AddHandler pullDetector.Compression, AddressOf onCompression
        End If
        lastRefreshed = DateTime.Now
        page += 1
        cargado = True
        setPG(False)
    End Sub

    Sub ShowAutor(ByVal sender As Object, ByVal e As EventArgs) Handles llAutores.SelectionChanged
        If llAutores.SelectedItem IsNot Nothing Then
            Dim item As autorElement = llAutores.SelectedItem
            NavigationService.Navigate(New Uri("/ShowAutor.xaml?nid=" + item.nid, UriKind.RelativeOrAbsolute))
        End If
        
    End Sub

    Sub onCompression(sender As Object, e As CompressionEventArgs)
        If DateTime.Now - lastRefreshed > TimeSpan.FromSeconds(1.5) And e.Type = CompressionType.Bottom Then
            setPG(True)
            LoadAutores()
        End If
    End Sub

End Class
