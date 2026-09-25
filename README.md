# OrderProcessingLab

Лабораторная работа №4 — рефакторинг и модульное тестирование.
Часть сквозного проекта TechStore: логика оформления и отмены заказа,
вынесенная в отдельный тестируемый модуль без зависимости от БД и UI.

## Структура

- `OrderProcessing/` — тестируемый проект (Domain + Services)
- `OrderProcessing.Tests/` — xUnit + Moq, покрытие через coverlet

## Запуск тестов с покрытием

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Threshold=100 /p:ThresholdType=line,branch /p:ThresholdStat=total
```

Сборка в GitHub Actions **падает**, если покрытие строк или ветвей опускается ниже 100% —
порог задан прямо в команде `dotnet test` через MSBuild-свойства coverlet.msbuild.
