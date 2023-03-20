namespace OneBella.Views

open System
open Avalonia
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Interactivity
open Avalonia.Markup.Xaml
open OneBella
open OneBella.UseCases
open OneBella.ViewModels
open LiteDB
open Avalonia.Input
open System.Windows.Input


type MainWindow () as this = 
    inherit Window ()

    let mutable fly = Unchecked.defaultof<Flyout>
    do this.InitializeComponent()
    do this.Opened |> Observable.add (fun _ -> this.OpenConnectionWindowCLick(this, null))
    do this.Closed |> Observable.add (fun _ ->
         let vm = this.DataContext :?> MainWindowViewModel
         Repo.disconnect()
         for d in vm.DbItems do
            if d.IsConnected then
               d.Disconnect()
         )
    do this.KeyDown |> Observable.add (fun arg ->
        if (arg.Key = Key.F5) then
            let vm = this.DataContext :?> MainWindowViewModel
            if not (vm.SelectedTab = Unchecked.defaultof<ScriptViewModel> ) then
                let t = vm.SelectedTab.RunCommand :> ICommand
                t.Execute(null)
                )

    member private x.ScriptTabFlyoutOpened(sender:obj, _:EventArgs) =
        fly <- sender :?> Flyout

    member private x.ScriptTabFlyoutClickYes(sender:obj, _:RoutedEventArgs) =
        let main = x.DataContext :?> MainWindowViewModel
        if (main.Tabs.Count > 1)then
            use tab = sender :?> Button |> (fun b -> b.DataContext :?> ScriptViewModel)
            ignore(main.Tabs.Remove tab)
        fly.Hide()

    member private x.ScriptTabFlyoutClickNo(_:obj, _:RoutedEventArgs) =
        fly.Hide()

    member private x.OpenConnectionWindowCLick(_:obj, _:RoutedEventArgs)=
        let rec showAddWindow mainWindow conVm=
            async {
            let w = AddConnectionWindow(conVm)
            do! w.ShowDialog(mainWindow) |> Async.AwaitTask
            let! con = w.SelectFileTask |> Async.AwaitTask
            let vm = x.DataContext :?> MainWindowViewModel
            try
               conVm.Error <- "" 
               vm.Connect(con)
               StoredConnUseCase.create Repo.getDb
               |> StoredConnUseCase.save con

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
                //let savedConnections = Repo.getConnSettings()
                let uc =StoredConnUseCase.create Repo.getDb
                let p = StoredConnUseCase.loadAll uc |> Seq.toArray
                let vm =ConnectionViewModel(p)
                do! showAddWindow desktop.MainWindow vm
            | _ -> ()
        }
        run() |> Async.StartImmediateAsTask |> ignore


    member private this.InitializeComponent() =
#if DEBUG
        this.AttachDevTools()
#endif
        AvaloniaXamlLoader.Load(this)
