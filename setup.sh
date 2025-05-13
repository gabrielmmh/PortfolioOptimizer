#!/bin/bash

set -e

echo ""
echo "🔧 Setup do PortfolioOptimizer"
echo ""

# Atualizar pacotes
echo "📦 Atualizando pacotes..."
sudo apt update
echo ""

# Instalar make se não existir
if ! command -v make &> /dev/null
then
    echo "🛠 Instalando make..."
    sudo apt install -y make
    echo ""
else
    echo "✔ make já instalado"
    echo ""
fi

# Instalar .NET SDK se não existir
if ! command -v dotnet &> /dev/null
then
    echo "🛠 Instalando .NET SDK..."
    sudo apt install -y dotnet-sdk-9.0
    echo ""
else
    echo "✔ .NET SDK já instalado"
    echo ""
fi

# Restaurar dependências
echo "🔄 Restaurando dependências..."
dotnet restore
echo ""

echo "✔ Setup concluído"
echo "👉 Próximo passo: make build && make run"
echo ""
