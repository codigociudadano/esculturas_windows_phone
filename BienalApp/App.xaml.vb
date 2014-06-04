Imports System.Diagnostics
Imports System.Resources
Imports System.Windows.Markup

Partial Public Class App
    Inherits Application

    ''' <summary>
    ''' Proporcionar acceso sencillo al marco raíz de la aplicación telefónica.
    ''' </summary>
    ''' <returns>Marco raíz de la aplicación telefónica.</returns>
    Public Shared Property RootFrame As PhoneApplicationFrame
    Public Shared Property baseurl As String = "http://dev.resistenciarte.org"

    ''' <summary>
    ''' Constructor para el objeto Application.
    ''' </summary>
    Public Sub New()
        ' Inicialización XAML estándar
        InitializeComponent()

        ' Inicialización especifica del teléfono
        InitializePhoneApplication()

        ' Inicialización del idioma
        InitializeLanguage()

        ' Mostrar información de generación de perfiles gráfica durante la depuración.
        If Debugger.IsAttached Then
            ' Mostrar los contadores de velocidad de marcos actual.
            Application.Current.Host.Settings.EnableFrameRateCounter = True

            ' Mostrar las áreas de la aplicación que se están volviendo a dibujar en cada marco.
            'Application.Current.Host.Settings.EnableRedrawRegions = True

            ' Habilitar el modo de visualización de análisis de no producción,
            ' que muestra áreas de una página que se entregan a la GPU con una superposición coloreada.
            'Application.Current.Host.Settings.EnableCacheVisualization = True


            ' Impedir que la pantalla se apague mientras se realiza la depuración deshabilitando
            ' la detección de inactividad de la aplicación.
            ' Precaución: solo debe usarse en modo de depuración. Las aplicaciones que deshabiliten la detección de inactividad del usuario seguirán en ejecución
            ' y consumirán energía de la batería cuando el usuario no esté usando el teléfono.
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled
        End If
    End Sub

    ' Código para ejecutar cuando la aplicación se inicia (p.ej. a partir de Inicio)
    ' Este código no se ejecutará cuando la aplicación se reactive
    Private Sub Application_Launching(ByVal sender As Object, ByVal e As LaunchingEventArgs)
    End Sub

    ' Código para ejecutar cuando la aplicación se activa (se trae a primer plano)
    ' Este código no se ejecutará cuando la aplicación se inicie por primera vez
    Private Sub Application_Activated(ByVal sender As Object, ByVal e As ActivatedEventArgs)
    End Sub

    ' Código para ejecutar cuando la aplicación se desactiva (se envía a segundo plano)
    ' Este código no se ejecutará cuando la aplicación se cierre
    Private Sub Application_Deactivated(ByVal sender As Object, ByVal e As DeactivatedEventArgs)
    End Sub

    ' Código para ejecutar cuando la aplicación se cierra (p.ej., al hacer clic en Atrás)
    ' Este código no se ejecutará cuando la aplicación se desactive
    Private Sub Application_Closing(ByVal sender As Object, ByVal e As ClosingEventArgs)
    End Sub

    ' Código para ejecutar si hay un error de navegación
    Private Sub RootFrame_NavigationFailed(ByVal sender As Object, ByVal e As NavigationFailedEventArgs)
        If Diagnostics.Debugger.IsAttached Then
            ' Ha habido un error de navegación; interrumpir el depurador
            Diagnostics.Debugger.Break()
        End If
    End Sub

    Public Sub Application_UnhandledException(ByVal sender As Object, ByVal e As ApplicationUnhandledExceptionEventArgs) Handles Me.UnhandledException

        ' Mostrar información de generación de perfiles gráfica durante la depuración.
        If Diagnostics.Debugger.IsAttached Then
            Diagnostics.Debugger.Break()
        Else
            e.Handled = True
            MessageBox.Show(e.ExceptionObject.Message & Environment.NewLine & e.ExceptionObject.StackTrace,
                            "Error", MessageBoxButton.OK)
        End If
    End Sub

#Region "Inicialización de la aplicación telefónica"
    ' Evitar inicialización doble
    Private phoneApplicationInitialized As Boolean = False

    ' No agregar ningún código adicional a este método
    Private Sub InitializePhoneApplication()
        If phoneApplicationInitialized Then
            Return
        End If

        ' Crear el marco pero no establecerlo como RootVisual todavía; esto permite que
        ' la pantalla de presentación permanezca activa hasta que la aplicación esté lista para la presentación.
        RootFrame = New PhoneApplicationFrame()
        AddHandler RootFrame.Navigated, AddressOf CompleteInitializePhoneApplication

        ' Controlar errores de navegación
        AddHandler RootFrame.NavigationFailed, AddressOf RootFrame_NavigationFailed

        'Controlar solicitudes de restablecimiento para borrar la pila de retroceso
        AddHandler RootFrame.Navigated, AddressOf CheckForResetNavigation

        ' Asegurarse de que no volvemos a inicializar
        phoneApplicationInitialized = True
    End Sub

    ' No agregar ningún código adicional a este método
    Private Sub CompleteInitializePhoneApplication(ByVal sender As Object, ByVal e As NavigationEventArgs)
        ' Establecer el objeto visual raíz para permitir que la aplicación se presente
        If RootVisual IsNot RootFrame Then
            RootVisual = RootFrame
        End If

        ' Quitar este controlador porque ya no es necesario
        RemoveHandler RootFrame.Navigated, AddressOf CompleteInitializePhoneApplication
    End Sub

    Private Sub CheckForResetNavigation(ByVal sender As Object, ByVal e As NavigationEventArgs)
        ' Si la aplicación ha recibido una navegación 'reset', tenemos que comprobarlo
        ' en la siguiente navegación para ver si se debe restablecer la pila de páginas
        If e.NavigationMode = NavigationMode.Reset Then
            AddHandler RootFrame.Navigated, AddressOf ClearBackStackAfterReset
        End If
    End Sub

    Private Sub ClearBackStackAfterReset(ByVal sender As Object, ByVal e As NavigationEventArgs)
        ' Anular registro del evento para que no se vuelva a llamar
        RemoveHandler RootFrame.Navigated, AddressOf ClearBackStackAfterReset

        ' Borrar solo la pila de navegaciones 'new' (hacia delante) y 'refresh'
        If e.NavigationMode <> NavigationMode.New And e.NavigationMode <> NavigationMode.Refresh Then
            Return
        End If

        ' Por coherencia de la IU, borrar toda la pila de páginas
        While Not RootFrame.RemoveBackEntry() Is Nothing
            ' no hacer nada
        End While
    End Sub
#End Region

    ' Inicializar la fuente y la dirección de flujo de la aplicación según se define en sus cadenas de recursos traducidas.
    '
    ' Para asegurarse de que la fuente de la aplicación está alineada con sus idiomas admitidos y que
    ' FlowDirection para todos esos idiomas sigue su dirección tradicional, ResourceLanguage
    ' y ResourceFlowDirection se debe inicializar en cada archivo resx para que estos valores coincidan con ese
    ' referencia cultural del archivo. Por ejemplo:
    '
    ' AppResources.es-ES.resx
    '    El valor de ResourceLanguage debe ser "es-ES"
    '    El valor de ResourceFlowDirection debe ser "LeftToRight"
    '
    ' AppResources.ar-SA.resx
    '     El valor de ResourceLanguage debe ser "ar-SA"
    '     El valor de ResourceFlowDirection debe ser "RightToLeft"
    '
    ' Para obtener más información sobre cómo traducir aplicaciones para Windows Phone, consulta http://go.microsoft.com/fwlink/?LinkId=262072.
    '
    Private Sub InitializeLanguage()
        Try
            ' Establecer la fuente para que coincida con el idioma definido por
            ' Cadena de recursos ResourceLanguage para cada idioma admitido.
            '
            ' Recurrir a la fuente del idioma neutro si el idioma
            ' del teléfono no se admite.
            '
            ' Si se produce un error del compilador, falta ResourceLanguage
            ' el archivo de recursos.
            RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage)

            ' Establecer FlowDirection de todos los elementos del marco raíz según
            ' en la cadena de recursos ResourceFlowDirection para cada
            ' idioma admitido.
            '
            ' Si se produce un error del compilador, falta ResourceFlowDirection
            ' el archivo de recursos.
            Dim flow As FlowDirection = DirectCast([Enum].Parse(GetType(FlowDirection), AppResources.ResourceFlowDirection), FlowDirection)
            RootFrame.FlowDirection = flow
        Catch
            ' Si se detecta aquí una excepción, lo más probable es que se deba a
            ' ResourceLanguage no se ha establecido correctamente en un idioma admitido
            ' o ResourceFlowDirection se ha establecido en un valor distinto de LeftToRight
            ' o RightToLeft.

            If Debugger.IsAttached Then
                Debugger.Break()
            End If

            Throw
        End Try
    End Sub

End Class