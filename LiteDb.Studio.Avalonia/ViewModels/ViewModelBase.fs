namespace OneBella.ViewModels

open ReactiveUI

type ViewModelBase() =
    inherit ReactiveObject()

// member x.RaiseAndSet<'T>( old:byref<'T>, newVal:'T, [<CallerMemberNameAttribute>] ?property :string) =
//     x.RaiseAndSetIfChanged<'T, 'T>(old, newVal, property)
