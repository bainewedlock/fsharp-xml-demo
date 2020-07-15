open System
open System.IO
open System.Xml
open System.Xml.Serialization
open System.Runtime.Serialization
open System.Diagnostics

// requires: reference to System.Xml.Serialization
let naiveApproach (ob:'a) =
    let s = new System.Xml.Serialization.XmlSerializer(typeof<'a>)
    use writer = new StringWriter()
    s.Serialize(writer, ob)
    writer.ToString()

// requires: reference to System.Xml.Serialization
let naiveApproachWithCustomRootElement (ob:'a) =
    let root = XmlRootAttribute()
    root.ElementName <- "MyData"
    let s = new System.Xml.Serialization.XmlSerializer(typeof<'a>,root)
    use writer = new StringWriter()
    s.Serialize(writer, ob)
    writer.ToString()

// requires: reference to System.Runtime.Serialization
let datacontractApproach (ob:'a) =
    let ser = new DataContractSerializer(typeof<'a>)
    use sw = new StringWriter()
    use xw = new XmlTextWriter(sw)
    xw.Formatting <- Formatting.Indented
    ser.WriteObject(xw, ob)
    sw.ToString()

// requires: install-package FsPickler
let withPicklerLibrary ob = 
    let serializer = MBrace.FsPickler.FsPickler.CreateXmlSerializer(indent = true)
    use writer = new StringWriter()
    serializer.Serialize(writer, ob)
    writer.ToString()

let testObject = [| 1; 2; 3; 4; 5|]

let currentlyUsedRam () =
    use proc = Process.GetCurrentProcess()
    use PC = new PerformanceCounter()
    PC.CategoryName <- "Process"
    PC.CounterName <- "Working Set - Private"
    PC.InstanceName <- proc.ProcessName
    PC.NextValue() |> int

let benchmark title callback =
    let started = DateTimeOffset.Now
    let n = 1000
    for _ in [1..n] do callback testObject |> ignore
    let finished = DateTimeOffset.Now
    [
        sprintf "### %s" title
        sprintf "time for %d loops: %A" n (finished - started)
        callback testObject
        ""
    ]
    |> List.iter (printfn "%s")

[<EntryPoint>]
let main argv =
    benchmark "naiveApproach" naiveApproach
    benchmark "naiveApproachWithCustomRootElement" naiveApproachWithCustomRootElement
    benchmark "datacontractApproach" datacontractApproach
    benchmark "withPicklerLibrary" withPicklerLibrary
    0
