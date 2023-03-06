namespace LiteDb.Studio.Avalonia.Views

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Interactivity
open Avalonia.Markup.Xaml
open LiteDb.Studio.Avalonia.ViewModels

type ConnectionControl() as this =
    inherit UserControl()

    let vm = ConnectionViewModel()
    do AvaloniaXamlLoader.Load(this)
    do this.DataContext <- vm

    member x.VieModel = vm
    member private this.SelectDbFileCLick(send: obj, args: RoutedEventArgs) =
       let test() =
            async {
                match Application.Current.ApplicationLifetime with
                | :? IClassicDesktopStyleApplicationLifetime as desktop ->
                    let dg = OpenFileDialog()
                    dg.AllowMultiple <- false
                    dg.Title = "select the liteDb file"
                    let! files = dg.ShowAsync(desktop.MainWindow) |> Async.AwaitTask
                    ()
                | _ -> ()
            }

       test() |> Async.StartImmediateAsTask |> ignore
       ()
