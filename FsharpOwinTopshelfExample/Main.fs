namespace FsharpOwinTopshelfExample

open Owin
open Microsoft.Owin.Hosting
open System
open System.Web.Http
open Newtonsoft.Json.Serialization
open Topshelf

open System.Reflection
[<assembly: AssemblyTitle("SimpleWebAnalytics")>]()

module Main =
    let owinStartup (app:IAppBuilder) =
        let config = new HttpConfiguration()
        // don't emit data as XML
        config.Formatters.Remove config.Formatters.XmlFormatter |> ignore
        // use the JSON serializer for F# types etc.
        config.Formatters.JsonFormatter.SerializerSettings.ContractResolver <- DefaultContractResolver() 
        let route = config.Routes.MapHttpRoute(
                                            "Default",
                                            "api/{controller}/{id}" )
        route.Defaults.Add("id", RouteParameter.Optional )
 
        app.UseWebApi(config) |> ignore

    type WebAnalyticsService() =
        let hostUrl = "http://localhost:9000/"

        [<DefaultValue>] val mutable private app : IDisposable
        
        interface ServiceControl with
            member x.Start hc =   
                x.app <- WebApp.Start(hostUrl, owinStartup)
                true

            member x.Stop hc =
                x.app.Dispose()
                true
                
    [<EntryPoint>]
    let main argv =
        with_topshelf <| fun conf ->
            run_as_network_service conf
            naming_from_this_asm conf

            service conf <| fun _ -> new WebAnalyticsService()