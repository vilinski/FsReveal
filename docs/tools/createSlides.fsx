#r "nuget: FsReveal"
#r "nuget: FSharp.Formatting.Literate.dll"
#r "nuget: Fake.IO.FileSystem"
#r "nuget: Fake.Core.Trace"

open Fake.Core
open Fake.IO.FileSystemOperators
open System.IO

open FsReveal

let root = Path.Combine(__SOURCE_DIRECTORY__,"../../")
let slidesDir = root @@ "/docs/slides"
let outDir = root @@ "/docs/output/samples/"
FsReveal.FsRevealHelper.RevealJsFolder <- Path.Combine(root,"paket-files/fsprojects/reveal.js")
FsReveal.FsRevealHelper.TemplateFile <- Path.Combine(root,"src/FsReveal/template.html")

let targetFCIS = Path.Combine(root,@"packages/build/FAKE/tools/FSharp.Compiler.Interactive.Settings.dll")
if not (System.IO.File.Exists(targetFCIS)) then
    System.IO.File.Copy(Path.Combine(root,@"bin/FSharp.Compiler.Interactive.Settings.dll"), targetFCIS)


let copyStylesheet() =
    try
        Shell.copyFile (outDir @@ "css\custom.css") (slidesDir @@ "custom.css")
    with
    | exn -> Trace.traceImportant <| sprintf "Could not copy stylesheet: %s" exn.Message

let copyPics() =
    try
      !! (slidesDir @@ "images/*.*")
      |> CopyFiles (outDir @@ "images")
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
|> Seq.map fileInfo
|> Seq.iter generateFor
