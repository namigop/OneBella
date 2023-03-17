namespace OneBella.Views

open Avalonia
open Avalonia.Controls
open Avalonia.Markup.Xaml
open OneBella.ViewModels
open LiteDB

type AddConnectionWindow(conVm:ConnectionViewModel) as this=
    inherit Window()

  
    do AvaloniaXamlLoader.Load(this)
    do
        let c = this.FindControl<ConnectionControl>("ConnectionControl")
        c.DataContext <- conVm
        c.VieModel.Close <- (fun() -> this.Close())

    new() = AddConnectionWindow(ConnectionViewModel(Array.empty))

    member x.SelectFileTask
        with get() =
            let c = this.FindControl<ConnectionControl>("ConnectionControl")
            c.VieModel.SelectFileTask
