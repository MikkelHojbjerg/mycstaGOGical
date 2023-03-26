#r "nuget: Newtonsoft.Json, 13.0.3"
#r "nuget: FSharp.Data"
open System.Diagnostics
open Newtonsoft.Json
open System
open System.IO
open FSharp
open FSharp.Data
open System.Web
#load "valhalla.fsx"
open Valhalla

//  ********     *******     ********    **    **     *******  
//  /**/////    /**////**    /**/////    //**  **    /**////** 
//  /**         /**   /**    /**          //****     /**   /** 
//  /*******    /*******     /*******      //**      /*******  
//  /**////     /**///**     /**////        /**      /**///**  
//  /**         /**  //**    /**            /**      /**  //** 
//  /**         /**   //**   /********      /**      /**   //**
//  //          //     //    ////////       //       //     // 

//Freyr (The god of rain and sunshine) gives us information about the weather
//Develop by sockmaster27, Jonas, Ali and SjakalUngen

//Gets data from a
let fetchFromA (area: string) =
    //Uses url from a and adds encoded area sting 
    let url = heimdall.urlA + HttpUtility.UrlEncode(area)
    let response = Http.RequestString($"{url}", httpMethod = "POST", headers = [ "Authorization", $"Bearer {heimdall.token}" ])
    printfn $"Response from A: {response}"
    response

//let fetchFromATime (area: string, forwardToThePast: string) =
//    let url = heimdall.urlA + (area.Replace (" ", "%20"))
//    let response = Http.RequestString($"{url}", query = ["time= ", $"{forwardToThePast}"], httpMethod = "POST", headers = [ "Authorization", $"Bearer {heimdall.token}" ])
//    printfn $"Response from A: {response}"
//    response

//Sends data collected from a to b
let sendToB (data: string) =
    let response = Http.RequestString($"{heimdall.urlB}", httpMethod = "POST", body = TextRequest data, headers = [ "Authorization", $"Bearer {heimdall.token}" ])
    printfn $"Response from B: {response}"


//let mutable lastChecked = Map.empty 
//let forwardToThePast (area: string) =
//    let d = if lastChecked.ContainsKey area then lastChecked.[area] else new DateTime(2023, 03, 25, 23, 00, 00)
//    lastChecked <- Map.add area (d.Subtract (new TimeSpan ( 1, 0, 0))) lastChecked 
//    let respond = fetchFromATime(area, urd.toIsoStringNoSec(d))
//    let (json, updated) = (kvasir.translate (respond, area))
//    sendToB json

//mimir.runAllHist (heimdall.areas, forwardToThePast)
        
//Checks for updated data
let mutable lastUpdated = Map.empty
let run (area: string) =
    let response = fetchFromA area
    //Parse CSV to Json
    let (json, updated) = (kvasir.translate (response, area))
    let changed = if lastUpdated.ContainsKey area then updated <>  lastUpdated.[area] else true

    //If data is changes it will be send to b
    if changed then
        sendToB json
        lastUpdated <- Map.add area updated lastUpdated
        printfn "changed"
    else 
        printfn "not changed"
    
    (changed, updated)

//See runAll in vahalla.fsx
mimir.runAll (heimdall.areas, run)

