namespace OneBella.Views

open System
open Avalonia
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Interactivity
open Avalonia.Markup.Xaml
open OneBella.ViewModels
open LiteDB
open Avalonia.Input
open System.Windows.Input


type MainWindow () as this = 
    inherit Window ()

    let mutable fly = Unchecked.defaultof<Flyout>
    do this.InitializeComponent()
    do this.Opened |> Observable.add (fun arg -> this.OpenConnectionWindowCLick(this, null))
    do this.KeyDown |> Observable.add (fun arg ->
        if (arg.Key = Key.F5) then
            let vm = this.DataContext :?> MainWindowViewModel
            if not (vm.SelectedTab = Unchecked.defaultof<ScriptViewModel> ) then
                let t = vm.SelectedTab.RunCommand :> ICommand
                t.Execute(null)
                )

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
        let rec showAddWindow mainWindow conVm=
            async {
            let w = AddConnectionWindow(conVm)
            do! w.ShowDialog(mainWindow) |> Async.AwaitTask
            let! con = w.SelectFileTask |> Async.AwaitTask
            let vm = x.DataContext :?> MainWindowViewModel
            try
               conVm.Error <- "" 
               vm.Connect(con)
            with
            | exc -> 
              let err = exc.Message
              conVm.Error <- err
              do! showAddWindow mainWindow conVm
              
            }
                

        let run() =
            async {
            match Application.Current.ApplicationLifetime with
            | :? IClassicDesktopStyleApplicationLifetime as desktop ->
                do! showAddWindow desktop.MainWindow (ConnectionViewModel(ConnectionString()))  
            | _ -> ()
        }
        run() |> Async.StartImmediateAsTask |> ignore


    member private this.InitializeComponent() =
#if DEBUG
        this.AttachDevTools()
#endif
        AvaloniaXamlLoader.Load(this)
