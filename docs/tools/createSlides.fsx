#r "nuget: FSharp.Formatting"
#r "nuget: FSharp.Formatting.Literate"
#r "nuget: Fake.IO.FileSystem"
#r "nuget: Fake.Core.Trace"
#r "../../src/FsReveal/bin/Debug/netcoreapp3.1/FsReveal.dll"
// #r "nuget: FsReveal"

open Fake.Core
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open System.IO

open FsReveal

let root = Path.Combine(__SOURCE_DIRECTORY__,"../../")
let slidesDir = root @@ "/docs/slides"
let outDir = root @@ "/docs/output/samples/"
FsRevealHelper.RevealJsFolder <- Path.Combine(root,"paket-files/fsprojects/reveal.js")
FsRevealHelper.TemplateFile <- Path.Combine(root,"src/FsReveal/template.html")

// let targetFCIS = Path.Combine(root,@"packages/build/FAKE/tools/FSharp.Compiler.Interactive.Settings.dll")
// let targetFCIS = Path.Combine(root,@".nuget/packages/fsharp.formatting/9.0.1/lib/netstandard2.1//FSharp.Compiler.Interactive.Settings.dll")
// if not (File.Exists(targetFCIS)) then
//     File.Copy(Path.Combine(root,@"lib/FSharp.Compiler.Interactive.Settings.dll"), targetFCIS)


let copyStylesheet() =
    try
        Shell.copyFile (outDir @@ "css\custom.css") (slidesDir @@ "custom.css")
    with
    | exn -> Trace.traceImportant <| sprintf "Could not copy stylesheet: %s" exn.Message

let copyPics() =
    try
      !! (slidesDir @@ "images/*.*")
      |> Shell.copyFiles (outDir @@ "images")
    with
    | exn -> Trace.traceImportant <| sprintf "Could not copy picture: %s" exn.Message

let generateFor (file:FileInfo) =
    try
        copyPics()
        let rec tryGenerate trials =
            try
                FsReveal.GenerateFromFile(file.FullName, outDir)
            with
            | exn when trials > 0 -> tryGenerate (trials - 1)
            | exn ->
                Trace.traceImportant <| sprintf "Could not generate slides for: %s" file.FullName
                Trace.traceImportant exn.Message

        tryGenerate 3

        copyStylesheet()
    with
    | :? FileNotFoundException as exn ->
        Trace.traceImportant <| sprintf "Could not copy file: %s" exn.FileName



!! (slidesDir @@ "*.md")
++ (slidesDir @@ "*.fsx")
|> Seq.map FileInfo
|> Seq.iter generateFor
