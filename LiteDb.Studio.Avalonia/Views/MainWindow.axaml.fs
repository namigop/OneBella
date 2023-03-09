namespace LiteDb.Studio.Avalonia.Views

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Interactivity
open Avalonia.Markup.Xaml
open LiteDb.Studio.Avalonia.ViewModels


type MainWindow () as this = 
    inherit Window ()

    do this.InitializeComponent()
    do this.Opened |> Observable.add (fun arg -> this.OpenConnectionWindowCLick(this, null))

    member private this.OpenConnectionWindowCLick(send:obj, args:RoutedEventArgs)=
        let run() =
            async {
            match Application.Current.ApplicationLifetime with
            | :? IClassicDesktopStyleApplicationLifetime as desktop ->
                let w = AddConnectionWindow()
                do! w.ShowDialog(desktop.MainWindow) |> Async.AwaitTask
                let! con = w.SelectFileTask |> Async.AwaitTask
                let vm = this.DataContext :?> MainWindowViewModel
                vm.Connect(con)
                ()
            | _ -> ()
        }
        run() |> Async.StartImmediateAsTask |> ignore


    member private this.InitializeComponent() =
#if DEBUG
        this.AttachDevTools()
#endif
        AvaloniaXamlLoader.Load(this)
