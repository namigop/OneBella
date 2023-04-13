namespace OneBella.Views

open System
open System.Reactive
open System.Windows.Input
open Avalonia
open Avalonia.Controls
open Avalonia.Markup.Xaml
open OneBella.ViewModels

type ScriptControl() as this=
    inherit UserControl()

    do AvaloniaXamlLoader.Load(this)
    do
        let t = this.FindControl<TreeDataGrid>("TreeDataGridResults")
        t.DoubleTapped
        |> Observable.add (fun r ->
            match r.Source with
            | :? StyledElement as i ->
               match (i.DataContext) with
               | :? BsonItem as item ->
                     (item.EditCommand :> ICommand).Execute(Unit.Default)
               | _ -> ()
            | _ -> ())
