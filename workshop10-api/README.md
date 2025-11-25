# CampusWorkshops API — Starter for Workshop

Pré-requisitos:

* .NET 8 SDK instalado

Comandos básicos:

```bash
dotnet restore
dotnet run
```

Depois de rodar, abra: https://localhost:5001/swagger

Objetivos do encontro 1

* Entender o contrato REST (recursos, rotas, status codes).
* Implementar/ajustar `GET /api/workshops`, `GET /api/workshops/{id}` e codar o `POST /api/workshops` (com validação e `201 Created + Location`).
* Ver erros formatados como `application/problem+json`.
