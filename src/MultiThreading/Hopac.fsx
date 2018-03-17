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
#r "../../packages/Hopac/lib/net45/Hopac.Core.dll"
#r "../../packages/Hopac/lib/net45/Hopac.Platform.dll"
#r "../../packages/Hopac/lib/net45/Hopac.dll"
open Deedle
open FSharp.Data
open XPlot.GoogleCharts
open XPlot.GoogleCharts.Deedle
open Hopac
open Hopac.Infixes

type Product = { 
  Id : int
  Name : string
}

// int -> Job<Product>
let getProduct id = job {
  
  // Delay in the place of DB query logic for brevity
  do! timeOutMillis 2000

  return {Id = id; Name = "My Awesome Product"}
}

type Review = {
  ProductId : int
  Author : string
  Comment : string
}

// int -> Job<Review list>
let getProductReviews id = job {
  
  // Delay in the place of an external HTTP API call
  do! timeOutMillis 3000
  
  return [
    {ProductId = id; Author = "John"; Comment = "It's awesome!"}
    {ProductId = id; Author = "Sam"; Comment = "Great product"}
  ]
}


type ProductWithReviews = {
  Id : int
  Name : string
  Reviews : (string * string) list
}

// int -> Job<ProductWithReviews>
let getProductWithReviews id = job {
  let! product, reviews = getProduct id <*> getProductReviews id
  return {  
    Id = id
    Name = product.Name
    Reviews = reviews |> List.map (fun r -> r.Author,r.Comment)
  }
}
#time "on"
getProductWithReviews 1 |> run
#time "off"