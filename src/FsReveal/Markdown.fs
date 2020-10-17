[<AutoOpen>]
module internal FsReveal.Markdown

open System
open FSharp.Formatting.Literate
open FSharp.Formatting.Markdown

let getPresentation (doc : LiterateDocument) =
    /// get properties, a list of (key,value) from
    /// [[Span[Literal "key : value"]]]
    let getProperties (spans : list<list<_>>) =
        let extractProperty paragraphs =
            match paragraphs with
            | [ Span(l, _) ] ->
                match l with
                | [ Literal(v, _) ] when v.Contains(":") ->
                    let colonPos = v.IndexOf(':')
                    let key = v.Substring(0, colonPos).Trim()
                    let value = v.Substring(colonPos + 1).Trim()
                    (key, value)
                | _ -> failwithf "Invalid Presentation property: %A" l
            | _ -> failwithf "Invalid Presentation property: %A" paragraphs
        spans |> List.map extractProperty

    // main section is separated by ***
    let sections = splitByHorizontalRule '*' doc.Paragraphs

    let properties,slideData =
        let map,slideData =
            match sections.Head with
            | [ ListBlock(_, spans, _) ] -> getProperties spans |> Map.ofList,sections.Tail
            | _ -> Map.empty,sections

        let add key v map =
            match Map.tryFind key map with
            | None -> Map.add key v map
            | _ -> map

        let properties =
            map
            |> add "title" "Presentation"
            |> add "description" ""
            |> add "author" "unkown"
            |> add "theme" "night"
            |> add "transition" "default"
        properties,slideData

    let wrappedInSection (properties:Map<_,_>) paragraphs =
        let attributes = properties |> Seq.map (fun kv -> sprintf "%s=\"%s\"" kv.Key kv.Value)
        let section = sprintf "<section %s>" (String.Join(" ", attributes))
        InlineBlock(section, None, None) :: paragraphs @ [ InlineBlock("</section>", None, None) ]

    let getParagraphsFromSlide slide =
        match slide with
        | Simple(slideData)
        | Nested([slideData]) -> wrappedInSection slideData.Properties slideData.Paragraphs
        | Nested(nestedSlides) ->
            nestedSlides
            |> List.collect (fun slideData -> wrappedInSection slideData.Properties slideData.Paragraphs)
            |> wrappedInSection Map.empty

    let extractSlide paragraphs =
        let extractSlideData paragraphs =
            let properties, data =
                match paragraphs with
                | ListBlock(_, spans, _) :: data ->
                    try
                        getProperties spans, data
                    with _ -> [], paragraphs
                | _ -> [], paragraphs

            { Properties = properties |> Map.ofList
              Paragraphs = data }

        // sub-section is separated by ---
        let nestedSlides =
            paragraphs
            |> splitByHorizontalRule '-'
            |> List.map extractSlideData

        match nestedSlides with
        | [ slideData ] -> Simple slideData
        | _ -> Nested nestedSlides

    let slides = List.map extractSlide slideData
    let paragraphs = List.collect getParagraphsFromSlide slides
    { Properties = properties
      Slides = slides
      Document = doc.With(paragraphs = paragraphs) }
