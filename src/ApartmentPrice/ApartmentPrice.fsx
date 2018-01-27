// Before running any code, invoke Paket to get the dependencies.
//
// You can either build the project (Ctrl + Alt + B in VS) or run
// '.paket/paket.bootstrap.exe' and then '.paket/paket.exe install'
// (if you are on a Mac or Linux, run the 'exe' files using 'mono')
//
// Once you have packages, use Alt+Enter (in VS) or Ctrl+Enter to
// run the following in F# Interactive. You can ignore the project
// (running it doesn't do anything, it just contains this script)
#load "..\..\packages/FsLab/FsLab.fsx"

open Deedle
open FSharp.Data
open XPlot.GoogleCharts
open XPlot.GoogleCharts.Deedle

let searchUrl = "http://dom.gratka.pl/mieszkania-do-wynajecia/lista/lodzkie,lodz.html"

type Gratka = HtmlProvider<"http://dom.gratka.pl/mieszkania-do-wynajecia/lista/lodzkie,lodz.html">

let advertisements = Gratka.Load("http://dom.gratka.pl/mieszkania-do-wynajecia/lista/lodzkie,lodz,40,d_0,li,sr.html")

advertisements.Lists.``Mieszkania do wynajęcia Łódź - łódzkie 2``.Values