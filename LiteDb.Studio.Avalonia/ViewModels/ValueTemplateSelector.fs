namespace LiteDb.Studio.Avalonia.ViewModels

open System
open Avalonia.Controls
open Avalonia.Controls.Templates
open System.Collections.Generic
open Avalonia.Metadata
type ValueTemplateSelector() =
    let mutable content = new Dictionary<string, IDataTemplate>()

   //This Dictionary should store our shapes. We mark this as [Content],
   //so we can directly add elements to it later.
    [<Content>]
    member x.AvailableTemplates
       with get()  = content

    interface IDataTemplate with
        // Build the DataTemplate here
        member x.Build(param:obj) : IControl =
            let item = param :?> BsonItem
            //if (item = null) then
            //    raise (new ArgumentNullException(nameof(param)))
            let key =
                let isJson = item.Type = "string" && (item.Value.StartsWith("{") ||item.Value.StartsWith("["))
                if isJson && content.ContainsKey("Json") then "Json"
                elif item.Type = "document" && content.ContainsKey("Doc") then "Doc"
                else "Others"
            x.AvailableTemplates[key].Build(param); // finally we look up the provided key and let the System build the DataTemplate for us

        // Check if we can accept the provided data
        member x.Match(data:obj):bool =  data :? BsonItem
