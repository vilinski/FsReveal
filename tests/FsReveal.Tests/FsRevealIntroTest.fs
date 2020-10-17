module FsReveal.FsRevealIntroTest

open FsReveal
open NUnit.Framework
open FsUnit

[<Test>]
let ``can read FsReveal intro``() =
    FsReveal.GenerateOutputFromMarkdownFile("Index.md", "." ,"index.html")

    System.IO.File.Exists "index.html" |> shouldEqual true


[<Test>]
let ``can create intro twice``() =
    FsReveal.GenerateOutputFromMarkdownFile("Index.md", ".", "index.html")
    FsReveal.GenerateOutputFromMarkdownFile("Index.md", ".", "sample.html")

    System.IO.File.Exists "index.html" |> shouldEqual true
    System.IO.File.Exists "sample.html" |> shouldEqual true