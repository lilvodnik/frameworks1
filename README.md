Минимальная веб-служба на с# (ASP.NET Core) представляющая собой мини-библиотеку
Данная служба работает через Rest.Api и предоставляет 3-эндпоита для создания книг, возврата бибилиотеки и возврата книги по id 
Конвеер middlewere отвечает за:
1. Превращение исключений в json-ответ через ExceptionHandlingMiddleware
2. Генерирует уникальный идентификатор запроса через RequestIdMiddleware
3. Логгирует начало и конец запроса через LoggingMiddleware
4. Замеряет время выполнения через TimingMiddleware

Вызов rest.api осуществляется через терминал после запуска приложения
curl http://localhost:5000/api/books - получение всех книг
```
curl -X POST http://localhost:5000/api/books \
  -H "Content-Type: application/json" \
  -d '{"title":"Мастер и Маргарита","author":"Булгаков","price":500}'
```
 - создание новой книги
curl http://localhost:5000/api/books/1 - получить книги по id
