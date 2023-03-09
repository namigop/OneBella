namespace OneBella.Views

open System
open Avalonia
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Interactivity
open Avalonia.Markup.Xaml
open OneBella.ViewModels


type MainWindow () as this = 
    inherit Window ()

    let mutable fly = Unchecked.defaultof<Flyout>
    do this.InitializeComponent()
    do this.Opened |> Observable.add (fun arg -> this.OpenConnectionWindowCLick(this, null))

    member private x.ScriptTabFlyoutOpened(sender:obj, e:EventArgs) =
        fly <- sender :?> Flyout
        // fly.Content :?> Border
        // |> fun b -> b.FindControl<Button>("BtnYes")
        // |> fun b ->
        //     let main = x.DataContext :?> MainWindowViewModel
        //     b.IsEnabled <- main.Tabs.Count > 1

    member private x.ScriptTabFlyoutClickYes(sender:obj, e:RoutedEventArgs) =
        let main = x.DataContext :?> MainWindowViewModel
        if (main.Tabs.Count > 1)then
            use tab = sender :?> Button |> (fun b -> b.DataContext :?> ScriptViewModel)
            ignore(main.Tabs.Remove tab)
        fly.Hide()

    member private x.ScriptTabFlyoutClickNo(sender:obj, e:RoutedEventArgs) =
        fly.Hide()

    member private x.OpenConnectionWindowCLick(send:obj, args:RoutedEventArgs)=
        let run() =
            async {
            match Application.Current.ApplicationLifetime with
            | :? IClassicDesktopStyleApplicationLifetime as desktop ->
                let w = AddConnectionWindow()
                do! w.ShowDialog(desktop.MainWindow) |> Async.AwaitTask
                let! con = w.SelectFileTask |> Async.AwaitTask
                let vm = x.DataContext :?> MainWindowViewModel
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
