#r "nuget: FSharp.Data"
open System.Diagnostics
open System.Text.Json
open System
open System.IO
open FSharp.Data
#load "valhalla.fsx"
open Valhalla


let fetchFromA (area: string) =
    let url = njord.urlA + (area.Replace (" ", "%20"))
    Http.RequestString($"{url}", httpMethod = "POST", headers = [ "Authorization", $"Bearer {njord.token}" ])

let sendToB (data: string) =
    let response = Http.RequestString($"{njord.urlB}", httpMethod = "POST", body = TextRequest data, headers = [ "Authorization", $"Bearer {njord.token}" ])
    printfn $"Response from B: {response}"


let mutable lastJson = ""
let run () =
        for area in njord.areas do
            let response = fetchFromA area
            let json = (thor.toJson (response, area))
            if json <> lastJson then
                sendToB json
                lastJson <- json
                printfn "changed"
                printfn "%s\n\n%s" json lastJson
            else
                printfn "not changed"
