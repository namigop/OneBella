namespace OneBella.ViewModels


open Avalonia.Controls
open Avalonia.Controls.Templates
open System.Collections.Generic
open Avalonia.Metadata

type DbItemTemplateSelector() =
    let mutable content = new Dictionary<string, IDataTemplate>()

    //This Dictionary should store our shapes. We mark this as [Content],
    //so we can directly add elements to it later.
    [<Content>]
    member x.AvailableTemplates = content

    interface IDataTemplate with
        // Build the DataTemplate here
        member x.Build(param: obj) : Control =
            let item = param :?> DbItem
            let key = if item.IsCollection then "Table" else "Database"
            x.AvailableTemplates[ key ].Build(param) // finally we look up the provided key and let the System build the DataTemplate for us

        // Check if we can accept the provided data
        member x.Match(data: obj) : bool = data :? DbItem
