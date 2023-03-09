namespace OneBella.Views

open System
open AvaloniaEdit.Document
open AvaloniaEdit.Folding
open System.Collections.Generic


type CharFoldingStrategy(openingChar:char, closingChar:char) =

    let createNewFoldings(document:ITextSource) =
        let newFoldings = new List<NewFolding>()
        let startOffsets = new Stack<int>()
        let  mutable lastNewLineOffset = 0
        let openingBrace = openingChar
        let closingBrace = closingChar

        for i in 0 .. document.TextLength-1 do
            let c = document.GetCharAt(i)
            if (c = openingBrace) then
                startOffsets.Push(i)

            elif (c = closingBrace && startOffsets.Count > 0) then
                let startOffset = startOffsets.Pop()
                // don't fold if opening and closing brace are on the same line
                if (startOffset < lastNewLineOffset) then
                   newFoldings.Add(new NewFolding(startOffset, i + 1))

            elif (c = '\n' || c = '\r') then
                lastNewLineOffset <- i + 1
            else
                ()

        newFoldings.Sort(Comparison<NewFolding>(fun a b -> a.StartOffset.CompareTo(b.StartOffset)))
        newFoldings

    let createNewFoldings (document:TextDocument) (firstErrorOffset: outref<int>) =
        firstErrorOffset <- -1;
        createNewFoldings document;

    let updateFoldings (manager:FoldingManager)  (document:TextDocument) =
        let mutable firstErrorOffset = 0
        let newFoldings = createNewFoldings document &firstErrorOffset
        manager.UpdateFoldings(newFoldings, firstErrorOffset)

    member x.UpdateFoldings(manager, document) =
        updateFoldings manager document
