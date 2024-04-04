namespace OneBella.Views


open System
open System.Xml
open Avalonia;
open Avalonia.Controls;
open Avalonia.Data;
open Avalonia.Markup.Xaml
open Avalonia.Threading;
open AvaloniaEdit
open AvaloniaEdit.Document;
open AvaloniaEdit.Folding;
open AvaloniaEdit.Highlighting;
open AvaloniaEdit.Highlighting.Xshd

type SqlTextEditor() as this=
    inherit UserControl()

    do
        AvaloniaXamlLoader.Load(this)
        let editor = this.FindControl<TextEditor>("Editor")
        editor.Document <- new TextDocument ( Text = "" );
        editor.TextChanged |> Event.add (fun _ -> this.Text <- editor.Text)
        use resource = typeof<SqlTextEditor>.Assembly.GetManifestResourceStream("OneBella.Resources.sql.xshd")
        if not(resource = null) then
           use reader = new XmlTextReader(resource)
           editor.SyntaxHighlighting <- HighlightingLoader.Load(reader, HighlightingManager.Instance);


    static let  OnCoerceText (d:AvaloniaObject) arg =
         let sender = d :?> SqlTextEditor
         let editor = sender.FindControl<TextEditor>("Editor")
         if not(arg = editor.Text)  then
            editor.Text <- arg
         arg
    static let TextProperty : StyledProperty<string> =
        AvaloniaProperty.Register<UserControl, string>(
            "Text",
            "",
            false,
            BindingMode.TwoWay,
            null,
            OnCoerceText);

    //static member TextProperty = tp
    member x.Text
        with get() =  this.GetValue(TextProperty)
        and set (v:string) = this.SetValue(TextProperty, v) |> ignore;
