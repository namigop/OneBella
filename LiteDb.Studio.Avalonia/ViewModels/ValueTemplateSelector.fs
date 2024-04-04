namespace OneBella.ViewModels

open Avalonia.Controls
open Avalonia.Controls.Templates
open System.Collections.Generic
open Avalonia.Metadata

type ValueTemplateSelector() =
    let mutable content = new Dictionary<string, IDataTemplate>()

    //This Dictionary should store our shapes. We mark this as [Content],
    //so we can directly add elements to it later.
    [<Content>]
    member x.AvailableTemplates = content

    interface IDataTemplate with
        // Build the DataTemplate here
        member x.Build(param: obj) : Control =
            let item = param :?> BsonItem

            let key =
                let isJson =
                    item.Type = "string"
                    && (item.Value.StartsWith("{") || item.Value.StartsWith("["))

                if isJson && content.ContainsKey("JsonValue") then
                    "JsonValue"
                elif item.Type = "array" && content.ContainsKey("ArrayValue") then
                    "ArrayValue"
                elif item.Type = "document" && content.ContainsKey("DocValue") then
                    "DocValue"
                elif item.Type = "bool" && content.ContainsKey("BoolValue") then
                    "BoolValue"
                elif item.Type = "string" && content.ContainsKey("StringValue") then
                    "StringValue"
                elif
                    (item.Type = "decimal"
                     || item.Type = "double"
                     || item.Type = "int"
                     || item.Type = "long")
                    && content.ContainsKey("NumberValue")
                then
                    "NumberValue"
                elif content.ContainsKey("OthersValue") then
                    "OthersValue"

                //nameSelection options
                elif item.Type = "document" && content.ContainsKey("Doc") then
                    "Doc"
                else
                    "Others"

            x.AvailableTemplates[ key ].Build(param) // finally we look up the provided key and let the System build the DataTemplate for us

        // Check if we can accept the provided data
        member x.Match(data: obj) : bool = data :? BsonItem
