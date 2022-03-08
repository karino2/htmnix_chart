open System
open System.Drawing
open System.IO
open System.Reflection
open System.Text.Json
open PhotinoNET

type Message = {Type: string; Body: string}

let sendMessage (wnd:PhotinoWindow) (message:Message) =
    let msg = JsonSerializer.Serialize(message)
    wnd.SendWebMessage(msg) |> ignore

let onMessage onload (wnd:Object) (message:string) =
    let pwnd = wnd :?>PhotinoWindow
    let msg = JsonSerializer.Deserialize<Message>(message)
    match msg.Type with
    | "notifyLoaded" -> (onload pwnd) // sendMessage pwnd {Type="showHtml"; Body=html}
    | "notifyCancel" -> Environment.Exit 1
    | "notifyDone" ->
        let result = JsonSerializer.Deserialize<string array>(msg.Body)
        result|> Array.iter (printfn "%s")
        Environment.Exit 0
    | "notifyDeb" -> printfn "deb: %s" msg.Body
    | _ -> failwithf "Unknown msg type %s" msg.Type

let launchBrowser onload  =
    let onFinish (results:string array) = ()
    let onCancel () = ()


    let asm = Assembly.GetExecutingAssembly()

    let load (url:string) (prefix:string) =
        let fname = url.Substring(prefix.Length)
        asm.GetManifestResourceStream($"htmnix_chart.assets.{fname}")

    let win = (new PhotinoWindow(null))

    win.LogVerbosity <- 0
    win.SetTitle("htmnix_chart")
        .SetUseOsDefaultSize(false)
        .Center()
        .RegisterCustomSchemeHandler("resjs",
            PhotinoWindow.NetCustomSchemeDelegate(fun sender scheme url contentType ->
                contentType <- "text/javascript"
                load url "resjs:"))
        .RegisterCustomSchemeHandler("rescss", 
            PhotinoWindow.NetCustomSchemeDelegate(fun sender scheme url contentType ->
                contentType <- "text/css"
                load url "rescss:")) |> ignore
                
    let asm = Assembly.GetExecutingAssembly()
    use stream = asm.GetManifestResourceStream("htmnix_chart.assets.index.html")
    use sr = new StreamReader(stream)
    let text = sr.ReadToEnd()
    // printfn "content: %s" text

    win.RegisterWebMessageReceivedHandler(System.EventHandler<string>(onMessage onload))
        .Center()
        .SetSize(new Size(1200, 700))
        .LoadRawString(text)
        .WaitForClose()

let readLines () =
    fun _ -> Console.ReadLine()
    |>  Seq.initInfinite
    |>  Seq.takeWhile ((<>) null) 
    |>  Seq.toList

[<EntryPoint>]
let main argv =
    (*

    let json =  """
{
    "type": "bar",
    "data": {
        "labels": ["Red", "Blue", "Yellow", "Green", "Purple", "Orange"],
        "datasets": [{
            "label": "# of Votes",
            "data": [12, 19, 3, 5, 2, 3],
            "backgroundColor": [
                "rgba(255, 99, 132, 0.2)",
                "rgba(54, 162, 235, 0.2)",
                "rgba(255, 206, 86, 0.2)",
                "rgba(75, 192, 192, 0.2)",
                "rgba(153, 102, 255, 0.2)",
                "rgba(255, 159, 64, 0.2)"
            ],
            "borderColor": [
                "rgba(255, 99, 132, 1)",
                "rgba(54, 162, 235, 1)",
                "rgba(255, 206, 86, 1)",
                "rgba(75, 192, 192, 1)",
                "rgba(153, 102, 255, 1)",
                "rgba(255, 159, 64, 1)"
            ],
            "borderWidth": 1
        }]
    },
    "options": {
        "scales": {
            "y": {
                "beginAtZero": true
            }
        }
    }
}    
    """

    let onload pwnd = 
        sendMessage pwnd {Type="showChart"; Body=json}

    launchBrowser onload
    0
    *)

    if argv.Length <> 0 then
        printfn "usage: htmnix_chart"
        printfn "Read json from stdin. No argument."
        1
    else
        let lines =  readLines ()

        let json = lines |> String.concat "\n"

        let onload pwnd = 
            sendMessage pwnd {Type="showChart"; Body=json}

        launchBrowser onload

        0 // return an integer exit code
