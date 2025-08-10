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

type JsonTextEditor() as this=
    inherit UserControl()

    [<DefaultValue>]val mutable foldingTimer:DispatcherTimer
    [<DefaultValue>]val mutable folding:CharFoldingStrategy
    [<DefaultValue>]val mutable foldingManager:FoldingManager


    do
        AvaloniaXamlLoader.Load(this)
        this.foldingTimer <- new DispatcherTimer (Interval = TimeSpan.FromSeconds(2L))
        let editor = this.FindControl<TextEditor>("Editor")
        editor.Document <- new TextDocument ( Text = "" );
        editor.TextChanged |> Event.add (fun _ -> this.Text <- editor.Text)
        this.folding <- CharFoldingStrategy('{', '}')
        this.foldingTimer.Tick |> Observable.add(fun _ ->
            if (this.foldingManager = null) then
                this.foldingManager <- FoldingManager.Install(editor.TextArea);

            if ( not(this.foldingManager = null) && editor.Document.TextLength > 0) then
                this.folding.UpdateFoldings(this.foldingManager, editor.Document)
            )

        this.foldingTimer.IsEnabled <- false
        use resource = typeof<JsonTextEditor>.Assembly.GetManifestResourceStream("OneBella.Resources.json.xshd")
        if not(resource = null) then
           use reader = new XmlTextReader(resource)
           editor.SyntaxHighlighting <- HighlightingLoader.Load(reader, HighlightingManager.Instance);


    static let  OnCoerceText (d:AvaloniaObject) arg =
         let sender = d :?> JsonTextEditor
         let editor = sender.FindControl<TextEditor>("Editor")
         if not(arg = editor.Text)  then
            editor.Text <- arg
            sender.foldingTimer.IsEnabled <- true
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
