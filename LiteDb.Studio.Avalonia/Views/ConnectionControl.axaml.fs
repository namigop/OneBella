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
       let run() =
            async {
                match Application.Current.ApplicationLifetime with
                | :? IClassicDesktopStyleApplicationLifetime as desktop ->
                    let dg = OpenFileDialog()
                    dg.AllowMultiple <- false
                    dg.Title = "select the liteDb file"
                    let w = this.Parent.Parent :?> Window
                    let! files = dg.ShowAsync(w) |> Async.AwaitTask
                    this.VieModel.Set(files)
                | _ -> ()
            }

       run() |> Async.StartImmediateAsTask |> ignore
