Imports System.IO.IsolatedStorage
Imports Windows.Devices.Geolocation
Imports System.Net.Http
Imports Newtonsoft.Json.Linq
Imports System.Windows.Media.Imaging


Partial Public Class idObra
    Inherits PhoneApplicationPage
    Partial Public Class esculturaElement
        Public Property Title As String
        Public Property ImageSource As BitmapImage
        Public Property nid As String
    End Class

    Dim lista As List(Of esculturaElement) = New List(Of esculturaElement)

    Public Sub New()
        InitializeComponent()
    End Sub

    Protected Overrides Sub OnNavigatedTo(e As NavigationEventArgs)
        MyBase.OnNavigatedTo(e)
        If llEsculturas.ItemsSource Is Nothing Then
            If IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent") Then
                If Convert.ToBoolean(IsolatedStorageSettings.ApplicationSettings("LocationConsent")) = True Then
                    hacerCosas()
                Else
                    Dim result = MessageBox.Show("Esta aplicación usa tu ubicación para detectar las esculturas cercanas. Estás de acuerdo?", "Ubicación", MessageBoxButton.OKCancel)
                    If result = MessageBoxResult.OK Then
                        IsolatedStorageSettings.ApplicationSettings("LocationConsent") = True
                        hacerCosas()
                    Else
                        NavigationService.Navigate(New Uri("/Main.xaml", UriKind.RelativeOrAbsolute))
                    End If
                End If
            Else
                Dim result = MessageBox.Show("Esta aplicación usa tu ubicación para detectar las esculturas cercanas. Estás de acuerdo?", "Ubicación", MessageBoxButton.OKCancel)
                If result = MessageBoxResult.OK Then
                    IsolatedStorageSettings.ApplicationSettings.Add("LocationConsent", True)
                    hacerCosas()
                Else
                    IsolatedStorageSettings.ApplicationSettings.Add("LocationConsent", False)
                    NavigationService.Navigate(New Uri("/Main.xaml", UriKind.RelativeOrAbsolute))
                End If
            End If
        End If
        
    End Sub

    Public Class latitudLongitud
        Public Property latitud As String
        Public Property longitud As String
    End Class


    Public Async Function getCoordinates() As System.Threading.Tasks.Task(Of latitudLongitud)
        Dim locator As Geolocator = New Geolocator
        locator.DesiredAccuracyInMeters = 5
        Try
            Dim latLong As New latitudLongitud
            Dim position As Geoposition = Await locator.GetGeopositionAsync(TimeSpan.FromMinutes(3), TimeSpan.FromSeconds(30))
            latLong.latitud = position.Coordinate.Latitude.ToString
            latLong.longitud = position.Coordinate.Longitude.ToString
            Return latLong
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Async Sub hacerCosas()
        tbUbicación.Visibility = System.Windows.Visibility.Visible
        tbTrayendo.Visibility = System.Windows.Visibility.Collapsed
        SystemTray.ProgressIndicator = New ProgressIndicator
        SystemTray.ProgressIndicator.IsIndeterminate = True
        SystemTray.ProgressIndicator.IsVisible = True
        Dim latlong As latitudLongitud = Await getCoordinates()
        If latlong IsNot Nothing Then
            tbUbicación.Visibility = System.Windows.Visibility.Collapsed
            tbTrayendo.Visibility = System.Windows.Visibility.Visible
            Dim http As HttpClient = New HttpClient
            Dim result As HttpResponseMessage = Nothing
            Try
                result = Await http.GetAsync(App.baseurl + "/api/v1/closest_nodes_by_coord?lat=" + latlong.latitud + "&lon=" + latlong.longitud + "&qty_nodes=10&dist=12000")
                result.EnsureSuccessStatusCode()
            Catch hre As HttpRequestException
                MessageBox.Show("Algo salió mal con el request!", "Whoa!", MessageBoxButton.OK)
                Exit Sub
            End Try
            Dim content As String = Await result.Content.ReadAsStringAsync
            Dim esculturas As JArray = JArray.Parse(content)
            If esculturas.Count > 0 Then
                For Each token As JToken In esculturas
                    Dim escultura As New esculturaElement
                    escultura.nid = token.Item("nid")
                    escultura.Title = token.Item("node_title").ToString.Trim
                    Try
                        result = Await http.GetAsync(App.baseurl + "/api/v1/node/" + escultura.nid)
                        result.EnsureSuccessStatusCode()
                    Catch hre As HttpRequestException
                        MessageBox.Show("Algo anduvo mal con el request :/")
                        Exit Sub
                    End Try
                    content = Await result.Content.ReadAsStringAsync
                    Dim jsonesc As JToken = JToken.Parse(content)
                    If jsonesc.Item("field_fotos").HasValues Then
                        Dim storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication
                        Dim uriFull As String = jsonesc.Item("field_fotos").Item("und").Item(0).Item("uri").ToString.Replace("public://", App.baseurl + "/sites/default/files/")
                        Dim nombreFoto As String = uriFull.Split("/")(uriFull.Split("/").Count - 1)
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
                                result = Await http.GetAsync(uriFull)
                                result.EnsureSuccessStatusCode()
                            Catch hre As HttpRequestException
                                MessageBox.Show("Algo anduvo mal con el request :/")
                                Exit Sub
                            End Try
                            Dim streamBM = Await result.Content.ReadAsStreamAsync
                            bmimage = New BitmapImage
                            bmimage.SetSource(streamBM)
                            Dim stream As IsolatedStorageFileStream = storage.CreateFile(nombreFoto)
                            Dim wb As WriteableBitmap = New WriteableBitmap(bmimage)
                            wb.SaveJpeg(stream, bmimage.PixelWidth, bmimage.PixelHeight, 0, 100)
                            stream.Close()
                        End If
                        escultura.ImageSource = bmimage
                    End If
                    lista.Add(escultura)
                Next
                tbTrayendo.Visibility = System.Windows.Visibility.Collapsed
                llEsculturas.ItemsSource = lista
                SystemTray.ProgressIndicator.IsVisible = False
            Else
                MessageBox.Show("Parece que no hay ninguna escultura alrededor!", "Esculturas", MessageBoxButton.OK)
                NavigationService.Navigate(New Uri("/Main.xaml?fromIdObra=yes", UriKind.RelativeOrAbsolute))
                SystemTray.ProgressIndicator.IsVisible = False
            End If
        Else
            Dim tryAgain = MessageBox.Show("Hubo un error al obtener tu ubicación, te gustaría tratar de nuevo?", "Whoops", MessageBoxButton.OKCancel)
            If tryAgain = MessageBoxResult.OK Then
                hacerCosas()
            End If
        End If
    End Sub

    Private Sub verEscultura(ByVal sender As Object, ByVal e As EventArgs) Handles llEsculturas.SelectionChanged
        If llEsculturas.SelectedItem IsNot Nothing Then
            Dim item As esculturaElement = llEsculturas.SelectedItem
            NavigationService.Navigate(New Uri("/Escultura.xaml?nid=" + item.nid.Trim, UriKind.RelativeOrAbsolute))
        End If
    End Sub

End Class
