namespace LiteDb.Studio.Avalonia.Views

open Avalonia
open Avalonia.Controls
open Avalonia.Markup.Xaml

type AddConnectionWindow() as this=
    inherit Window()

    do AvaloniaXamlLoader.Load(this)
    do
        let c = this.FindControl<ConnectionControl>("ConnectionControl")
        c.VieModel.Close <- (fun() -> this.Close())
    member x.SelectFileTask
        with get() =
            let c = this.FindControl<ConnectionControl>("ConnectionControl")
            c.VieModel.SelectFileTask
