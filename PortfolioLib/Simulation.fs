module PortfolioLib.Simulation

open PortfolioLib.Types
open System

let annualizeReturn (dailyReturns: ReturnPoint array) =
    let avg = dailyReturns |> Array.averageBy (fun r -> r.Return)
    avg * 252.0

let annualizeVolatility (dailyReturns: ReturnPoint array) =
    let returns = dailyReturns |> Array.map (fun r -> r.Return)
    let mean = Array.average returns
    let variance = returns |> Array.averageBy (fun x -> (x - mean) ** 2.0)
    Math.Sqrt(variance) * sqrt 252.0

let sharpeRatio (annualRet: float) (volatility: float) (riskFreeRate: float) =
    if volatility = 0.0 then 0.0
    else (annualRet - riskFreeRate) / volatility

let evaluatePortfolio (ticker: string) (returns: ReturnPoint array) (riskFreeRate: float) =
    let annRet = annualizeReturn returns
    let annVol = annualizeVolatility returns
    let sr = sharpeRatio annRet annVol riskFreeRate
    { Ticker = ticker; AnnualizedReturn = annRet; AnnualizedVolatility = annVol; SharpeRatio = sr }
