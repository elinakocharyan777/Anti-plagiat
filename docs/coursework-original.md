# Кочарян Элина Самвеловна БПИ-244
# AntiPlagiat  — микросервисная система проверки работ

Проект реализует MVP информационной системы для
- хранения присланных студентами файлов (работ)
- фиксации факта сдачи и формирования отчёта «плагиат/не плагиат»
- получения аналитики по всем сдачам по конкретному заданию

## Архитектура

Система состоит из 3 сервисов:

1) **ApiGateway** (порт **5036**) - единственная точка входа для клиента
- принимает запросы от пользователя
- вызывает **FileStorageService** для загрузки файла
- вызывает **AnalysisService** для создания сдачи и получения отчётов
- если внутренний сервис недоступен то возвращает **503 Service Unavailable**

2) **FileStorageService** (порт **5037**) - хранение и выдача файлов
- `POST /api/Files` - загрузка 
- `GET /api/Files/{id}` - скачивание

3) **AnalysisService** (порт **5027**) - хранение сдач, анализ (антиплагиат) и выдача отчётов
- использует SQLite (файл `analysis.db` внутри контейнера, сохраняется в volume)
- `POST /api/Submissions` - создать сдачу + сразу сформировать отчёт
- `GET /api/Submissions/by-assignment/{assignmentId}` - список сдач по заданию
- `GET /api/Submissions/{id}/report` - подробный отчёт по сдаче

## Алгоритм определения плагиата

**Идея:** две работы считаются совпадающими, если у них одинаковый **SHA256-хэш содержимого файла**

Для новой сдачи система делает:
1) скачивает файл из FileStorageService по `FileId`
2) вычисляет SHA256-хэш;
3) ищет в базе **более раннюю** сдачу по тому же `AssignmentId`, но от **другого** студента, с тем же хэшем
4) если такая сдача найдена то `IsPlagiarism = true`, `OriginalSubmissionId = <id более ранней сдачи>`


## User flow 

### 1- студент отправляет работу на проверку
1) Клиент -> ApiGateway: `POST /api/Works` (multipart/form-data: studentId, assignmentId, file)
2) ApiGateway -> FileStorageService: `POST /api/Files` (загрузка файла)
3) ApiGateway -> AnalysisService: `POST /api/Submissions` (создание сдачи + запуск анализа)
4) AnalysisService -> FileStorageService: `GET /api/Files/{fileId}` (скачивание и подсчёт SHA256)
5) ApiGateway -> клиент: ответ с `submissionId` + флагом `isPlagiarism`

### 2- преподаватель получает аналитику по заданию
1) Клиент -> ApiGateway: `GET /api/Works/{assignmentId}/reports`
2) ApiGateway -> AnalysisService: `GET /api/Submissions/by-assignment/{assignmentId}`
3) ApiGateway -> клиент: список сдач по заданию со статусами и `isPlagiarism`

### 3- преподаватель получает подробный отчёт по конкретной сдаче
1) Клиент -> ApiGateway: `GET /api/Works/submissions/{submissionId}/report`
2) ApiGateway -> AnalysisService: `GET /api/Submissions/{id}/report`
3) ApiGateway -> клиент: подробный отчёт (hash, isPlagiarism, originalSubmissionId, timestamps)

## Обработка ошибок 
- Если FileStorageService или AnalysisService не отвечает, ApiGateway возвращает `503 Service Unavailable`
  и сообщение о недоступности внутреннего сервиса

## Как запускать

#### через Visual Studio
1. Открой решение (Solution) в Visual Studio
2. ПКМ по Solution → **Properties**
3. Выбери **Multiple startup projects**
4. Для проектов:
   - `FileStorageService` → Start
   - `AnalysisService` → Start
   - `ApiGateway` → Start
5. Нажми **Apply → OK**
6. Запусти F5

#### через терминал
- Открываем 3 терминала в папке проекта
1) cd FileStorageService
dotnet run
2) cd AnalysisService
dotnet run
3) cd ApiGateway
dotnet run
- Открываем Swagger:
http://localhost:5036/swagger


## Swagger доступен по адресам:

- ApiGateway: http://localhost:5036/swagger

- AnalysisService: http://localhost:5027/swagger

- FileStorageService: http://localhost:5037/swagger

## Хранение 
- файлы работ хранятся в FileStorageService (Docker в volume)
- результаты анализа и сдачи хранятся в AnalysisService (SQLite `analysis.db`, Docker в volume)

## Примеры запросов 

### Загрузить работу (через Gateway)
`POST http://localhost:5036/api/Works`

Формат: `multipart/form-data`
- `studentId` (string)
- `assignmentId` (string)
- `file` (file)

Пример (Windows PowerShell):
```powershell
curl -X POST "http://localhost:5036/api/Works" `
  -F "studentId=s1" `
  -F "assignmentId=task-1" `
  -F "file=@./test.txt"

