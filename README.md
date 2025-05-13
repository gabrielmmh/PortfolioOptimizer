# Portfolio Optimizer

Este projeto realiza simulações de carteiras de investimento otimizadas usando ações do índice Dow Jones. O objetivo é identificar as combinações de ativos que resultam no maior índice de Sharpe, oferecendo a melhor relação risco-retorno.

## Requisitos e Dependências

* .NET SDK 8.0 ou superior

* Ferramenta `make`

* Ambiente Unix/Linux recomendado (pode rodar via WSL no Windows)

* Conexão com a internet para baixar os dados financeiros

## Como Instalar

Execute o script `setup.sh` para instalar automaticamente as dependências necessárias:

```bash
bash setup.sh
```

Esse script instala o .NET SDK, o utilitário `make`, e restaura todas as dependências do projeto.

## Como Executar

O projeto utiliza um `Makefile` que simplifica sua execução em diferentes modos:

* **Compilar o projeto**:

  ```bash
  make build
  ```

* **Executar em modo desenvolvimento** (simulações menores):

  ```bash
  make run-dev
  ```

* **Executar em modo produção** (simulações completas exigidas pelo trabalho):

  ```bash
  make run-prod
  ```

* **Publicar um executável otimizado**:

  ```bash
  make publish
  ```

* **Rodar o executável publicado em produção**:

  ```bash
  make run-publish
  ```

* **Limpar arquivos temporários e builds**:

  ```bash
  make clean
  ```

## Estrutura do Projeto

* `PortfolioApp`: Aplicação principal que gerencia as simulações e resultados.

* `PortfolioLib`: Biblioteca em F# com lógica funcional pura e otimizada, utilizando paralelização avançada e vetorização.

* `PortfolioFetcher`: Utilitário em C# que realiza download automático de dados financeiros via Stooq API.

* `data/2024`: Dados financeiros para treinamento (segundo semestre de 2024).

* `data/2025`: Dados financeiros para testes (primeiro trimestre de 2025).

* `Makefile`: Automação de build e execução do projeto.

* `setup.sh`: Script para instalação e configuração inicial do ambiente.

## Resultados Esperados e Comparações

Ao executar em modo produção, o projeto:

1. Baixa automaticamente os dados financeiros, se necessário.

2. Realiza simulações de combinações com 25 ativos dentre 30 disponíveis no índice Dow Jones, sorteando 1.000 carteiras possíveis por combinação.

3. Executa a simulação utilizando técnicas avançadas de paralelização (Parallel.ForEach em chunks) e vetorização SIMD (`Vector<float>`).

4. Exibe as cinco melhores carteiras simuladas, mostrando:

   * Índice de Sharpe
   * Retorno anualizado
   * Volatilidade anualizada
   * Distribuição percentual dos ativos

5. Testa a melhor carteira encontrada contra os dados reais do primeiro trimestre de 2025, apresentando desempenho real:

   * Retorno anualizado real
   * Volatilidade anualizada real
   * Índice de Sharpe real

6. Compara desempenho entre execuções sequenciais e paralelas, executando 5 vezes cada modelo, exibindo estatísticas como média, tempo mínimo e máximo.

### Resultados Reais Levantados

Com base em testes reais realizados na máquina de desenvolvimento com a build otimizada (publish), foi possível observar os seguintes resultados:

* **Execução Paralela (5 execuções):**

  * Tempo médio: 226.49 segundos

  * Tempo mínimo: 201.20 segundos

  * Tempo máximo: 268.66 segundos

* **Execução Sequencial (5 execuções):**

  * Tempo médio: 254.86 segundos

  * Tempo mínimo: 253.55 segundos

  * Tempo máximo: 256.76 segundos

### Conclusão

Embora a diferença entre o desempenho das versões paralela e sequencial não seja drasticamente grande, a implementação da paralelização trouxe uma melhoria significativa no tempo médio de execução (cerca de 11%). Isso demonstra que o uso estratégico de paralelização com chunks grandes já otimizados e vetorização SIMD aproveita ao máximo os recursos disponíveis da CPU, reduzindo o tempo total da simulação.