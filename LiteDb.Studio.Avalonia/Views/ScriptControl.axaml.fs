namespace LiteDb.Studio.Avalonia.Views

open Avalonia
open Avalonia.Controls
open Avalonia.Markup.Xaml

type ScriptControl() as this=
    inherit UserControl()

    do AvaloniaXamlLoader.Load(this)
