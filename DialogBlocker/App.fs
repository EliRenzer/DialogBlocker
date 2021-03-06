﻿namespace DialogBlocker.App

open System
open System.IO
open Autodesk.Revit.Attributes
open Autodesk.Revit.UI
open Autodesk.Revit.UI.Events


module hello =

    let log (line:string) =
        if Directory.Exists(@"C:\temp") |> not then Directory.CreateDirectory(@"C:\temp") |> ignore
        use wr = new StreamWriter(@"C:\temp\DialogBoxShowingEventArgs.txt", true, System.Text.Encoding.UTF8)
        wr.WriteLine(line)
        
    let DialogHandler = fun (e:DialogBoxShowingEventArgs) ->
        
        let te = e :?> TaskDialogShowingEventArgs        
        let now = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")
        let logmess mess = log (sprintf "%s; %s; %s; %s; %s; \n\r" mess now te.DialogId te.Message (e.GetType().FullName))

        let IsBlocked (mess:string) = 
            let blocked = ["Revit will use raster printing"; "The <in-session> print settings will be used"]
            blocked |> List.tryFind (fun m -> mess.Contains m )

        // supress dialog
        if (IsBlocked(te.Message).IsSome ) 
        then te.OverrideResult(int TaskDialogResult.Close ) |> ignore
             logmess "BLOCKED"
        else logmess "ALLOWED"

    //// IExternalApplication implement
    ////
    let OnStartup (app:UIControlledApplication) =
        // Dialog supress
        app.DialogBoxShowing.Add DialogHandler
        Result.Succeeded
    
    let OnShutdown (app:UIControlledApplication) =
        app.DialogBoxShowing.RemoveHandler |>ignore
        Result.Succeeded

// IExternalApplication
[<Transaction(TransactionMode.Manual)>]
type MainApp() = 
    interface IExternalApplication with
        member x.OnStartup(app) = hello.OnStartup app
        member x.OnShutdown(app) = hello.OnShutdown app
