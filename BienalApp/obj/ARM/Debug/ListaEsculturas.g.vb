﻿#ExternalChecksum("C:\Users\Carlos\Documents\Visual Studio 2013\Projects\BienalApp\BienalApp\ListaEsculturas.xaml","{406ea660-64cf-4c82-b6f0-42d48172a799}","FA06434CCF108F6A1C4F4743648C3FA9")
'------------------------------------------------------------------------------
' <auto-generated>
'     Este código fue generado por una herramienta.
'     Versión de runtime:4.0.30319.34014
'
'     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
'     se vuelve a generar el código.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict Off
Option Explicit On

Imports Microsoft.Phone.Controls
Imports System
Imports System.Windows
Imports System.Windows.Automation
Imports System.Windows.Automation.Peers
Imports System.Windows.Automation.Provider
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Ink
Imports System.Windows.Input
Imports System.Windows.Interop
Imports System.Windows.Markup
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Media.Imaging
Imports System.Windows.Resources
Imports System.Windows.Shapes
Imports System.Windows.Threading



<Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>  _
Partial Public Class ListaEsculturas
    Inherits Microsoft.Phone.Controls.PhoneApplicationPage
    
    Friend WithEvents LayoutRoot As System.Windows.Controls.Grid
    
    Friend WithEvents llEsculturas As Microsoft.Phone.Controls.LongListSelector
    
    Private _contentLoaded As Boolean
    
    '''<summary>
    '''InitializeComponent
    '''</summary>
    <System.Diagnostics.DebuggerNonUserCodeAttribute()>  _
    Public Sub InitializeComponent()
        If _contentLoaded Then
            Return
        End If
        _contentLoaded = true
        System.Windows.Application.LoadComponent(Me, New System.Uri("/BienalApp;component/ListaEsculturas.xaml", System.UriKind.Relative))
        Me.LayoutRoot = CType(Me.FindName("LayoutRoot"),System.Windows.Controls.Grid)
        Me.llEsculturas = CType(Me.FindName("llEsculturas"),Microsoft.Phone.Controls.LongListSelector)
    End Sub
End Class

