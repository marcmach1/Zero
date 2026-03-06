#!/bin/zsh
# Script para finalizar processos na porta 5119 e rodar dotnet run

PORT=5119
PID=$(lsof -ti :$PORT)
if [ ! -z "$PID" ]; then
  echo "Finalizando processo na porta $PORT (PID: $PID)..."
  kill $PID
else
  echo "Nenhum processo usando a porta $PORT."
fi

dotnet run
