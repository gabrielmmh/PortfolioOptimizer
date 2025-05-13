# Diretórios
PROJECT = PortfolioApp
OUTPUT_DIR = publish
RUNTIME = linux-x64

# Targets
.PHONY: build run-dev run-prod publish run-publish perf-publish clean

# Compilação normal (release)
build:
	dotnet build -c Release

# Rodar em modo desenvolvimento (5 ativos dentre 10)
run-dev: build
	dotnet run --project $(PROJECT)

# Rodar em modo produção (25 ativos dentre 30)
run-prod: build
	dotnet run --project $(PROJECT) -- --prod

# Publicação otimizada (self-contained, ReadyToRun, SingleFile)
publish:
	dotnet publish $(PROJECT) -c Release -r $(RUNTIME) --self-contained true /p:PublishReadyToRun=true /p:PublishSingleFile=true -o $(OUTPUT_DIR)

# Rodar o executável publicado em produção
run-publish: publish
	./$(OUTPUT_DIR)/PortfolioApp --prod

# Rodar com time
time-publish:
	time ./publish/PortfolioApp --prod

# Limpar builds, publish e artefatos
clean:
	dotnet clean
	rm -rf $(OUTPUT_DIR)
