module PortfolioLib.DataLoader

open System
open System.IO
open PortfolioLib.Types

let parseCsvLine (line: string) : DailyPrice option =
    let parts = line.Split(',')
    if parts.Length = 6 then
        match DateTime.TryParse(parts.[0]), Double.TryParse(parts.[1]), Double.TryParse(parts.[2]),
              Double.TryParse(parts.[3]), Double.TryParse(parts.[4]), Double.TryParse(parts.[5]) with
        | (true, date), (true, o), (true, h), (true, l), (true, c), (true, v) ->
            Some { Date = date; Open = o; High = h; Low = l; Close = c; Volume = v }
        | _ -> None
    else None

let loadPrices (filePath: string) : DailyPrice array =
    File.ReadLines(filePath)
    |> Seq.skip 1
    |> Seq.choose parseCsvLine
    |> Seq.toArray
    |> Array.sortBy (fun p -> p.Date)

let computeDailyReturns (prices: DailyPrice array) : ReturnPoint array =
    prices
    |> Array.pairwise
    |> Array.map (fun (prev, curr) ->
        { Date = curr.Date
          Return = (curr.Close - prev.Close) / prev.Close })

let dowJonesTickers =
    [| "AAPL"; "AMGN"; "AXP"; "BA"; "CAT"; "CRM"; "CSCO"; "CVX"; "DIS"; "DOW";
       "GS"; "HD"; "HON"; "IBM"; "INTC"; "JNJ"; "JPM"; "KO"; "MCD"; "MMM";
       "MRK"; "MSFT"; "NKE"; "PG"; "TRV"; "UNH"; "V"; "VZ"; "WBA"; "WMT" |]

let loadReturnsFromFile (filePath: string) : DailyPrice array option =
    if File.Exists filePath then
        Some (loadPrices filePath)
    else
        None

let loadReturnsForAll folder =
    Directory.GetFiles(folder)
    |> Array.Parallel.choose (fun file ->
        match loadReturnsFromFile file with
        | Some data ->
            let ticker = Path.GetFileNameWithoutExtension file
            if Array.contains ticker dowJonesTickers then
                let returns = computeDailyReturns data
                Some (ticker, returns)
            else None
        | None -> None)
    |> Map.ofArray
