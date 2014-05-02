Imports System.Net.Http
Imports Newtonsoft.Json.Linq
Imports System.Windows.Media.Imaging
Imports System.IO.IsolatedStorage



Partial Public Class autorElement
    Public Property Nombre As String
    Public Property Desc As String
    Public Property ImageSource As BitmapImage
    Public Property nid As String
End Class

Partial Public Class Autor
    Inherits PhoneApplicationPage


    Dim listaAutores As List(Of autorElement)
    Dim cargado As Boolean
    Public Sub New()
        InitializeComponent()
        listaAutores = New List(Of autorElement)
        cargado = False
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
        SystemTray.ProgressIndicator.IsVisible = val
    End Sub

    Private Async Sub LoadAutores()
        Dim http As New HttpClient
        Dim response = Await http.GetAsync(App.baseurl + "/api/v1/node?parameters[type]=autores")
        Dim content As String = Await response.Content.ReadAsStringAsync
        Dim array As JArray = JArray.Parse(content)
        Dim storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication
        For Each token As JToken In array
            Dim tempautor As autorElement = New autorElement
            tempautor.nid = token.Item("nid").ToString
            tempautor.Nombre = token.Item("title").ToString.Trim
            response = Await http.GetAsync(App.baseurl + "/api/v1/node/" + token.Item("nid").ToString)
            content = Await response.Content.ReadAsStringAsync
            Dim jAutor As JObject = JObject.Parse(content)
            tempautor.Desc = jAutor.Item("body").Item("und").Item(0).Item("value").ToString.Trim
            Dim urlFoto As String = jAutor.Item("field_fotos").Item("und").Item(0).Item("uri").ToString.Replace("public://", App.baseurl + "/sites/default/files/")
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
        llAutores.ItemsSource = listaAutores
        cargado = True
        setPG(False)
    End Sub

    Sub ShowAutor(ByVal sender As Object, ByVal e As EventArgs) Handles llAutores.SelectionChanged
        If llAutores.SelectedItem IsNot Nothing Then
            Dim item As autorElement = llAutores.SelectedItem
            NavigationService.Navigate(New Uri("/ShowAutor.xaml?nid=" + item.nid, UriKind.RelativeOrAbsolute))
        End If
        
    End Sub



End Class
