open System
open System.IO
open System.Diagnostics
open PortfolioFetcher
open PortfolioLib

// ──────────────────────────────────────────────────────────────
// utilitário simples para cronometrar blocos
let benchmark label f =
    let sw = Stopwatch.StartNew()
    let result = f ()
    sw.Stop()
    printfn $"{label}: {sw.Elapsed.TotalSeconds:F2}s"
    result

let inline timeSeconds (f: unit -> 'a) =
    let sw = Stopwatch.StartNew()
    let res = f ()
    sw.Stop()
    sw.Elapsed.TotalSeconds, res

// baixa dados se não existirem (ou faltarem arquivos)
let ensureDataExists year startD endD =
    let folder = Path.Combine("data", year.ToString())
    if not (Directory.Exists(folder)) || Directory.GetFiles(folder, "*.csv").Length < 30 then
        printfn $"⬇️ Baixando dados para {year}..."
        StooqApi.DownloadAsync(startD, endD, folder).GetAwaiter().GetResult()

// mede 5 execuções e devolve lista de tempos
let measure label f =
    printfn $"\n⏱️  {label} – 5 execuções:"
    [1..5]
    |> List.map (fun run ->
        let secs, _ = timeSeconds f
        printfn $"   • Exec #{run}: {secs:F2}s"
        secs)

let printStats name times =
    printfn $"{name} ➜ média: {List.average times:F2}s | min: {List.min times:F2}s | max: {List.max times:F2}s"
// ──────────────────────────────────────────────────────────────

[<EntryPoint>]
let main argv =
    let prodMode      = argv |> Array.exists (fun arg -> arg = "--prod")
    let riskFreeRate  = 0.05
    let combinationSize = if prodMode then 25 else 5
    let expectedTickers = 30

    // garante dados necessários
    ensureDataExists 2024 (DateTime(2024,8,1)) (DateTime(2024,12,31))
    ensureDataExists 2025 (DateTime(2025,1,1)) (DateTime(2025,3,31))

    // ────────── TREINAMENTO 2024 ──────────
    let returnsMap = DataLoader.loadReturnsForAll "data/2024"
    let tickers    = returnsMap |> Map.toList |> List.map fst
    printfn $"📊 {tickers.Length} ativos carregados."

    if prodMode && tickers.Length <> expectedTickers then
        failwithf "❌ Esperado %d ativos, mas carregou %d. Verifique a pasta data." expectedTickers tickers.Length

    let returnsByTickerDate =
        returnsMap
        |> Map.map (fun _ (arr: Types.ReturnPoint array) ->
            arr |> Array.map (fun r -> r.Date, r.Return) |> Map.ofArray)

    // simulação paralela principal
    let results =
        benchmark "🧵 Simulação Paralela" (fun () ->
            CombinationSimulator.simulateCombinationsParallel
                returnsByTickerDate tickers riskFreeRate combinationSize)

    // top-5
    let topResults =
        results
        |> List.sortByDescending (fun (_, _, _, s, _) -> s)
        |> List.truncate 5

    printfn "\n🏆 Melhores carteiras simuladas:\n"
    for i,(tks,annRet,vol,sharpe,wts) in topResults |> List.indexed do
        printfn $"#%d{i+1}: Sharpe: {sharpe:F3} | Retorno: {annRet*100.0:F2}%% | Volatilidade: {vol*100.0:F2}%%"
        Array.zip tks wts
        |> Array.iter (fun (t,w) -> printfn $" - {t}: {w*100.0:F2}%%")
        printfn ""

    // ────────── TESTE 1º TRI 2025 ──────────
    let q1ReturnsMap = DataLoader.loadReturnsForAll "data/2025"
    let returnsByTickerDateQ1 =
        q1ReturnsMap
        |> Map.map (fun _ (arr: Types.ReturnPoint array) ->
            arr |> Array.map (fun r -> r.Date, r.Return) |> Map.ofArray)

    let bestTickers,_,_,_,bestWeights = topResults.Head
    let returnsMatrixQ1 =
        CombinationSimulator.buildCombinationMatrix returnsByTickerDateQ1 bestTickers

    let (annQ1, volQ1, sharpeQ1) =
        CombinationSimulator.evaluatePortfolioFromWeights returnsMatrixQ1 bestWeights riskFreeRate

    printfn "\n📊 Resultados da melhor carteira no 1º trimestre de 2025:"
    printfn $" - Retorno anualizado: {annQ1*100.0:F2}%%"
    printfn $" - Volatilidade anualizada: {volQ1*100.0:F2}%%"
    printfn $" - Sharpe Ratio: {sharpeQ1:F3}"

    // ────────── BENCHMARK 5× PARAL vs 5× SEQ ──────────
    let parallelTimes =
        measure "Paralelo" (fun () ->
            CombinationSimulator.simulateCombinationsParallel
                returnsByTickerDate tickers riskFreeRate combinationSize |> ignore)

    let sequentialTimes =
        measure "Sequencial" (fun () ->
            CombinationSimulator.simulateCombinationsSequential
                returnsByTickerDate tickers riskFreeRate combinationSize |> ignore)

    printfn "\n📈 Resumo de tempos:"
    printStats "Paralelo " parallelTimes
    printStats "Sequencial" sequentialTimes

    0
