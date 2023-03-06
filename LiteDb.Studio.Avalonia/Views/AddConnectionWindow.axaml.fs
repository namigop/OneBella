namespace LiteDb.Studio.Avalonia.Views

open Avalonia
open Avalonia.Controls
open Avalonia.Markup.Xaml

type AddConnectionWindow() as this=
    inherit Window()

    do AvaloniaXamlLoader.Load(this)
