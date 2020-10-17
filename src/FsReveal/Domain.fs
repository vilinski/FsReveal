namespace FsReveal

open FSharp.Formatting.Literate
open FSharp.Formatting.Markdown

type SlideData =
    { Properties : Map<string,string>
      Paragraphs : MarkdownParagraph list }

type Slide =
    | Simple of SlideData
    | Nested of SlideData list

type Presentation =
    { Properties : Map<string,string>
      Slides : Slide list
      Document : LiterateDocument }
