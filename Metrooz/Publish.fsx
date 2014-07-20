#r "System.IO.Compression"
open System.IO
open System.IO.Compression

let isReleaseBuild = fsi.CommandLineArgs.[2] = "Release"
if isReleaseBuild then
    let prjDir = __SOURCE_DIRECTORY__
    let binDir = prjDir + "\\bin\\Release"
    let pubDir = prjDir + "\\Publish"
    let appName = "Metrooz"
    let dirSepChar = Path.DirectorySeparatorChar.ToString()

    //Cleaning Dir
    //Debug
    let pdbAndXmlFiles =
        [yield! Directory.EnumerateFiles(binDir, "*.dll", SearchOption.AllDirectories)
         yield! Directory.EnumerateFiles(binDir, "*.exe", SearchOption.AllDirectories)]
        |> Seq.map(fun path -> path.Substring(0, path.Length - Path.GetExtension(path).Length))
        |> Seq.collect(fun path -> [".pdb"; ".xml"] |> Seq.map(fun ex -> path + ex))
        |> Seq.where(fun path -> File.Exists(path))
    for filePath in pdbAndXmlFiles do
        File.Delete(filePath)
    //Blend
    Directory.Delete(binDir + "\\SampleData", true)

    //Publish
    let rec dirToZip zipPath targetDirPath targetFilePaths=
        use zipStrm = File.Create(zipPath)
        use zipArch = new ZipArchive(zipStrm, ZipArchiveMode.Create)
        targetFilePaths |> Seq.iter(fun path ->
            use fileStrm = File.OpenRead(path)
            let entryPath = appName + dirSepChar + path.Substring((targetDirPath + dirSepChar).Length)
            let entry = zipArch.CreateEntry(entryPath)
            use entryStrm = entry.Open()
            fileStrm.CopyTo(entryStrm)
        )
    ignore(Directory.CreateDirectory(pubDir))
    Directory.EnumerateFiles(binDir, "*", SearchOption.AllDirectories)
    |> Seq.where(fun path -> path.Contains(".vshost.exe") = false)
    |> dirToZip (pubDir + dirSepChar + appName + ".zip") binDir
