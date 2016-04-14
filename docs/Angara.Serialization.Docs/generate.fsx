#I "../../ext/nuget/FSharp.Formatting.CommandTool.2.14.1/tools"
#r "../../ext/nuget/FSharp.Formatting.CommandTool.2.14.1/tools/FSharp.CodeFormat.dll"
#r "../../ext/nuget/FSharp.Formatting.CommandTool.2.14.1/tools/FSharp.Literate.dll"
#r "../../ext/nuget/FSharp.Formatting.CommandTool.2.14.1/tools/FSharp.Markdown.dll"
#r "../../ext/nuget/FSharp.Formatting.CommandTool.2.14.1/tools/FSharp.MetadataFormat.dll"

open FSharp.Literate
open FSharp.MetadataFormat
open System.IO

let info = [ "project-name", "Angara.Serialization"
           ; "project-summary", "A library that facilitates serialization on both .NET and JavaScript platforms and communication between them"
           ; "project-author", "Microsoft Research" 
           ; "root", "." 
           ; "project-nuget", "https://www.nuget.org/packages?q=Angara.Serialization"
           ; "project-github", "https://github.com/Microsoft/Angara.Serialization"]

Directory.SetCurrentDirectory(__SOURCE_DIRECTORY__)
let templates = [ Path.GetFullPath "../templates"; Path.GetFullPath "../templates/reference" ]
let docTemplate = "docpage.cshtml"
let tips_path = Path.GetFullPath "../../ext/nuget/FSharp.Formatting.CommandTool.2.14.1/styles/tips.js"
let style_path = Path.GetFullPath "../../ext/nuget/FSharp.Formatting.CommandTool.2.14.1/styles/style.css"
let output_dir = Path.GetFullPath "../out"
let output path = Path.Combine(output_dir, Path.GetFileName path)
let content path = Path.Combine(output_dir, (Path.Combine("content", Path.GetFileName path)))
let src path = Path.Combine(Path.GetFullPath "../../docs/Angara.Serialization.Docs", path)


// prepare output directory
if Directory.Exists(output "") then Directory.Delete(output "", true)
Directory.CreateDirectory(output "")
Directory.CreateDirectory(content "")
for f in [tips_path; style_path] do
    printfn "%s" (content f)
    File.WriteAllText(content f, File.ReadAllText f)

// Set current directory to templates
Directory.SetCurrentDirectory("../templates")
printfn "Current directory is %s" (Directory.GetCurrentDirectory())

// Generates HTML from FSharp script files using F# Literate Programming.
// process script files

let fsi = FsiEvaluator()
for f in 
    [ src "index.fsx" 
    ; src "CustomSerializer.fsx" ] 
    do
    if File.Exists f then
        printfn "Processing %s" (Path.GetFullPath(f))
        Literate.ProcessScriptFile(f, 
                                   docTemplate, 
                                   Path.ChangeExtension(output f, "html"), 
                                   fsiEvaluator = fsi, 
                                   lineNumbers = false,
                                   replacements = info,
                                   layoutRoots = [ Path.Combine(__SOURCE_DIRECTORY__, "../templates") ])
    else printfn "No such file: %s" f

// Generates F# library documentation from inline comments.
//for assembly in [  src "bin/Debug/Angara.Serialization.dll"
//                   ]
//    do
//    if File.Exists assembly then
//        printfn "Processing %s" (Path.GetFullPath(assembly))
//        MetadataFormat.Generate 
//            ( assembly,
//              output "",
//              templates,
//              sourceRepo = "https://github.com/Microsoft/Angara.Serialization/tree/master",
//              sourceFolder = Path.GetFullPath(assembly),
//              markDownComments = true,
//              parameters = info)
//    else printfn "No such file: %s" assembly  
