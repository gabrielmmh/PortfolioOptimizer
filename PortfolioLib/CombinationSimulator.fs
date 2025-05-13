module PortfolioLib.CombinationSimulator

open System
open System.Numerics
open System.Threading.Tasks
open System.Collections.Concurrent
open PortfolioLib.Types
open PortfolioLib.Simulation
open PortfolioLib.WeightGenerator

let rec combinations k (lst: 'a list) =
    match k, lst with
    | 0, _ -> [ [||] ]
    | _, [] -> []
    | k, x::xs -> 
        let withX = combinations (k-1) xs |> List.map (fun ys -> Array.append [|x|] ys)
        let withoutX = combinations k xs
        withX @ withoutX

let buildCombinationMatrix (returnsByTickerDate: Map<string, Map<DateTime, float>>) (tickers: string array) =
    let dates = returnsByTickerDate.[tickers.[0]] |> Map.toArray |> Array.map fst
    dates
    |> Array.map (fun date -> tickers |> Array.map (fun t -> returnsByTickerDate.[t].[date]))

let inline dotProduct (a: float[]) (b: float[]) =
    let vectorSize = Vector<float>.Count
    let mutable acc = Vector(0.0)

    let upperBound = a.Length - (a.Length % vectorSize)
    for i in 0 .. vectorSize .. upperBound - 1 do
        let va = Vector(a, i)
        let vb = Vector(b, i)
        acc <- acc + (va * vb)

    let mutable result = Vector.Sum(acc)

    for i in upperBound .. a.Length - 1 do
        result <- result + a.[i] * b.[i]

    result

let evaluatePortfolioFromWeights (returnsMatrix: float[][]) (weights: float array) (riskFreeRate: float) =
    let dailyReturns =
        returnsMatrix
        |> Array.map (fun row -> dotProduct row weights)

    let annReturn = Array.average dailyReturns * 252.0
    let mean = Array.average dailyReturns
    let stddev = Math.Sqrt(Array.averageBy (fun r -> (r - mean) ** 2.0) dailyReturns) * sqrt 252.0
    let sharpe = if stddev = 0.0 then 0.0 else (annReturn - riskFreeRate) / stddev
    annReturn, stddev, sharpe


let simulateCombination (returnsByTickerDate: Map<string, Map<DateTime, float>>) (tickers: string array) (riskFreeRate: float) =
    let returnsMatrix = buildCombinationMatrix returnsByTickerDate tickers
    let weightsArray = generateMultipleWeights tickers.Length 1000

    weightsArray
    |> Array.Parallel.map (fun weights ->
        let res = evaluatePortfolioFromWeights returnsMatrix weights riskFreeRate
        res, weights)
    |> Array.maxBy (fun ((_, _, sharpe), _) -> sharpe)

let simulateCombinationsParallel (returnsByTickerDate: Map<string, Map<DateTime, float>>) (tickersToCombine: string list) (riskFreeRate: float) (combinationSize: int) =
    let combs = combinations combinationSize tickersToCombine |> List.toArray

    let chunkSize =
        let cores = Environment.ProcessorCount
        let baseSize = combs.Length / (cores * 4)
        max 500 baseSize

    let chunks = 
        combs
        |> Array.chunkBySize chunkSize

    let results = ConcurrentBag<_>()

    Parallel.ForEach(chunks, fun chunk ->
        let localResults =
            chunk
            |> Array.map (fun tickers ->
                let (annRet, stddev, sharpe), weights = simulateCombination returnsByTickerDate tickers riskFreeRate
                tickers, annRet, stddev, sharpe, weights)

        for res in localResults do
            results.Add(res)
    ) |> ignore

    results |> Seq.toList

let simulateCombinationsSequential (returnsByTickerDate: Map<string, Map<DateTime, float>>) (tickersToCombine: string list) (riskFreeRate: float) (combinationSize: int) =
    let combs = combinations combinationSize tickersToCombine
    combs
    |> List.map (fun tickers ->
        let (annRet, stddev, sharpe), weights = simulateCombination returnsByTickerDate tickers riskFreeRate
        tickers, annRet, stddev, sharpe, weights)
