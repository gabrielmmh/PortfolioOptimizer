module PortfolioLib.WeightGenerator

open System

/// Gera um vetor de n pesos aleatórios que:
/// - São >= 0
/// - Nenhum > 20%
/// - Soma = 1.0
let generateValidWeights (n: int) : float array =
    let rng = Random.Shared
    let rec tryGenerate () =
        let raw = Array.init n (fun _ -> rng.NextDouble())
        let total = Array.sum raw
        let normalized = raw |> Array.map (fun v -> v / total)
        if normalized |> Array.exists (fun v -> v > 0.20) then
            tryGenerate ()
        else
            normalized
    tryGenerate ()

/// Gera k vetores de pesos válidos (cada um com n ativos)
let generateMultipleWeights (n: int) (count: int) : float array array =
    Array.init count (fun _ -> generateValidWeights n)
