# University Course Management API

REST Web API на ASP.NET Core 8 для управления студентами, преподавателями, курсами и записями на курсы.
Хранилище — PostgreSQL через Entity Framework Core.

## Запуск

1. Установить .NET 8 SDK и запустить PostgreSQL:
   ```bash
   docker compose up -d
   ```
   (или использовать свой PostgreSQL и изменить `ConnectionStrings:DefaultConnection` в `appsettings.json`)
2. Запустить API:
   ```bash
   dotnet run
   ```
3. Открыть Swagger: http://localhost:5080/swagger

При первом запуске приложение само создаёт таблицы и заполняет их тестовыми данными.
Альтернатива — выполнить скрипт `Database/university_db.sql` (схема + тестовые данные):
```bash
psql -U postgres -d university_db -f Database/university_db.sql
```

## Архитектура

```
Client
  ↓ HTTP
Controller          (маршрутизация, HTTP-коды, без бизнес-логики)
  ↓
Service             (бизнес-правила, логирование, маппинг DTO)
  ↓
Repository          (доступ к данным)
  ↓
Entity Framework Core (ApplicationDbContext)
  ↓
PostgreSQL
```

```
UniversityApi
├── Controllers      Students, Teachers, Courses, Enrollments
├── Models           Student, Teacher, Course, Enrollment
├── DTO              Create / Update / Dto для каждой сущности
├── Data             ApplicationDbContext, Configurations (Fluent API), DataSeeder
├── Repositories     Interfaces / Implementations
├── Services         Interfaces / Implementations (бизнес-правила)
├── Mapping          MappingProfile (AutoMapper)
├── Middleware       ExceptionHandlingMiddleware
├── Common           ReturnResult<T>, ErrorCodes, Validation
├── Database         university_db.sql
└── Program.cs       DI, Swagger, конвейер
```

### Конфигурация модели EF Core
- **Conventions** — Primary Key (`BaseEntity.Id`), имена таблиц и FK.
- **Data Annotations** — `[Required]`, `[MaxLength]` в моделях.
- **Fluent API** — связи, Foreign Key, `OnDelete`, уникальные индексы, точность `Grade` (`Data/Configurations`).

### Lifetime зависимостей (Program.cs)
| Компонент | Lifetime | Почему |
|---|---|---|
| `ApplicationDbContext` | Scoped | один контекст на HTTP-запрос, не потокобезопасен |
| Repositories | Scoped | зависят от DbContext, должны жить не дольше него |
| Services | Scoped | зависят от репозиториев |
| AutoMapper | регистрирует библиотека (`AddAutoMapper`) | конфигурация маппинга — Singleton (неизменяема), `IMapper` легковесный |
| `ExceptionHandlingMiddleware` | Singleton (создаётся один раз) | не хранит состояние запроса |

## Формат ответа

Все endpoints возвращают `ReturnResult<T>`:
```json
{ "statusCode": 200, "isSuccess": true, "result": { }, "errorCode": null, "errorMessage": null, "traceId": "..." }
```
Ошибка валидации (400) дополнительно содержит поле `errors` с описанием по полям.
Непредвиденные ошибки → 500 `INTERNAL_SERVER_ERROR` без StackTrace; подробности — в логе.

## Endpoints

### Students — `/api/students`
| Метод | URL | Описание | Коды |
|---|---|---|---|
| GET | `/api/students` | все студенты | 200 |
| GET | `/api/students/{id}` | студент по Id | 200, 404 |
| GET | `/api/students/{id}/details` | студент вместе с курсами и оценками | 200, 404 |
| POST | `/api/students` | создать | 201, 400, 409 (email занят) |
| PUT | `/api/students/{id}` | изменить | 200, 400, 404, 409 |
| DELETE | `/api/students/{id}` | удалить (записи удаляются каскадно) | 200, 404 |

### Teachers — `/api/teachers`
| Метод | URL | Описание | Коды |
|---|---|---|---|
| GET | `/api/teachers` | все преподаватели | 200 |
| GET | `/api/teachers/{id}` | по Id | 200, 404 |
| POST | `/api/teachers` | создать | 201, 400, 409 |
| PUT | `/api/teachers/{id}` | изменить | 200, 400, 404, 409 |
| DELETE | `/api/teachers/{id}` | удалить | 200, 404, 409 (есть курсы) |

### Courses — `/api/courses`
| Метод | URL | Описание | Коды |
|---|---|---|---|
| GET | `/api/courses?name=&teacherId=&minCredits=&maxCredits=` | список с фильтрацией | 200, 400 |
| GET | `/api/courses/{id}` | по Id | 200, 404 |
| POST | `/api/courses` | создать (TeacherId обязателен) | 201, 400, 404 (нет преподавателя) |
| PUT | `/api/courses/{id}` | изменить | 200, 400, 404 |
| DELETE | `/api/courses/{id}` | удалить | 200, 404, 409 (есть записи) |

### Enrollments — `/api/enrollments`
| Метод | URL | Описание | Коды |
|---|---|---|---|
| GET | `/api/enrollments?studentId=&courseId=` | список записей | 200 |
| GET | `/api/enrollments/{id}` | запись по Id | 200, 404 |
| POST | `/api/enrollments` | записать студента на курс | 201, 400, 404 (нет студента/курса), 409 (повтор) |
| PUT | `/api/enrollments/{id}/grade` | изменить оценку (0–100) | 200, 400, 404 |
| DELETE | `/api/enrollments/{id}` | удалить студента с курса | 200, 404 |

## Бизнес-правила
1. Нельзя записать студента на несуществующий курс → 404 `COURSE_NOT_FOUND`.
2. Нельзя записать несуществующего студента → 404 `STUDENT_NOT_FOUND`.
3. Повторная запись на тот же курс → 409 `ENROLLMENT_ALREADY_EXISTS` (+ уникальный индекс в БД).
4. Курс с несуществующим преподавателем → 404 `TEACHER_NOT_FOUND`.
5. Несуществующий объект никогда не возвращается как успех → 404 с кодом ошибки.

## Валидация
Имя/фамилия обязательны (до 100 символов); Email обязателен, корректный формат, уникальный; дата рождения — не в будущем и не раньше 1900;
название курса обязательно; кредиты 1–10; оценка 0–100.

## Логирование
`ILogger` во всех сервисах и middleware: `Information` — создание/изменение/удаление, `Warning` — объект не найден и нарушения бизнес-правил,
`Error` — необработанные исключения. Логи выводятся в консоль.

## Сценарии для защиты (Swagger / Postman)
1. `GET /api/students` → 2. `POST /api/students` → 3. `GET /api/students/{id}` → 4. `PUT /api/students/{id}`
5. `POST /api/teachers` → 6. `POST /api/courses` с `teacherId` → 7. `POST /api/enrollments`
8. Повторный `POST /api/enrollments` → 409 → 9. `GET /api/students/{id}/details`
10. `GET /api/courses?minCredits=4&name=web` → 11. `DELETE /api/enrollments/{id}`
12. `GET /api/students/9999` → 404 → 13. `POST /api/students` с пустым Email / `credits: 50` → 400
14. Смотреть логи в консоли `dotnet run`.


---

# Задание 2: безопасность, роли и версионирование

Задание 2 — продолжение задания 1: CRUD, Repository, DTO, AutoMapper, `ReturnResult<T>` и `ExceptionHandlingMiddleware` сохранены, старые маршруты (`/api/courses`, `/api/students`, ...) работают как раньше.

## Состав решения

```
UniversityApi/      защищённый API (JWT + Identity + роли + версии v1/v2)
UniversityClient/   ASP.NET Core MVC клиент (IHttpClientFactory), ходит только по HTTP
```

## Запуск

1. Поднять PostgreSQL. **Схема изменилась (таблицы Identity), поэтому БД из задания 1 нужно пересоздать:**
   ```bash
   docker compose down -v
   docker compose up -d
   ```
2. Задать ключ подписи JWT **вне репозитория** (user-secrets), минимум 32 символа:
   ```bash
   cd UniversityApi
   dotnet user-secrets set "Jwt:Key" "замените-на-случайную-строку-не-короче-32-символов"
   ```
   Альтернатива — переменная окружения `Jwt__Key`. Без ключа приложение не стартует.
3. (Рекомендуется) создать миграции — один раз:
   ```bash
   dotnet tool install --global dotnet-ef
   dotnet ef migrations add InitialIdentity
   ```
   Если папки `Migrations` нет, приложение создаёт схему через `EnsureCreated`; если есть — применяет их через `Migrate()`.
4. Запустить API: `dotnet run` -> http://localhost:5080/swagger (выпадающий список версий v1 / v2, кнопка **Authorize** для JWT).
5. Запустить клиент (второй терминал): `cd UniversityClient && dotnet run` -> http://localhost:5200

Адрес API клиента — `ApiSettings:BaseUrl` в `UniversityClient/appsettings.json`. Разрешённые адреса CORS — `Cors:AllowedOrigins` в `UniversityApi/appsettings.json` (политика `ClientApps`: `http://localhost:5200`, `https://localhost:7200`).

## Тестовые пользователи и роли

Роли создаются при старте. Тестовые пользователи создаются только в Development, пароль — `Seed:Password` из `appsettings.Development.json` (`Passw0rd!`). Альтернатива для ролей — `Database/identity_roles.sql`.

| Пользователь | Роль | Связь |
|---|---|---|
| admin | Admin | — |
| teacher.gertsen | Teacher | Teacher «Elena Gertsen» (курсы 1 и 3) |
| teacher.abdoldina | Teacher | Teacher «Farida Abdoldina» (курс 2) |
| student.alex | Student | Student «Alex Smith» (Id 1) |
| student.aigerim | Student | Student «Aigerim Bekova» (Id 2) |

Новый пользователь через `POST /api/auth/register` получает только роль **Student** и не связан с записью Student: привязку и повышение прав выполняет только Admin (`PUT /api/admin/users/{id}/link`, `POST /api/admin/users/{id}/roles`).

## Матрица доступа

| Endpoint | Anonymous | Student | Teacher | Admin |
|---|---|---|---|---|
| GET courses (v1, v2) | да | да | да | да |
| POST/PUT/DELETE courses, students, teachers, enrollments(POST/DELETE) | 401 | 403 | 403 | да |
| GET teachers | 401 | да | да | да |
| GET students | 401 | 403 | 403 | да |
| GET students/{id}, students/{id}/details | 401 | только себя | студентов своих курсов | да |
| GET enrollments | 401 | только свои | только по своим курсам | все |
| PUT enrollments/{id}/grade | 401 | 403 | только свой курс | да |
| /api/admin/users/* | 401 | 403 | 403 | да |
| /api/auth/register, /api/auth/login | да | да | да | да |
| GET /api/auth/me | 401 | да | да | да |

Права проверяются на сервере: роль (`[Authorize(Policy = ...)]`) + принадлежность ресурса (`IResourceAccessService`).

## Аутентификация и авторизация

- **Аутентификация** отвечает на вопрос «кто ты?»: `POST /api/auth/login` проверяет пароль через Identity (`SignInManager.CheckPasswordSignInAsync`) и выдаёт JWT.
- **Авторизация** отвечает на вопрос «что тебе можно?»: по claims из JWT (роль, `teacher_id`, `student_id`) решается, разрешён ли доступ.
- Пример: `student.alex` успешно входит (аутентифицирован), но `DELETE /api/courses/1` даёт 403 (не авторизован). Без токена тот же запрос даёт 401.
- Проверка роли недостаточна: `teacher.gertsen` имеет роль Teacher, но оценку на курсе Abdoldina менять не может (403) — дополнительно проверяется, что курс принадлежит именно ему.
- Связка: Identity хранит пользователей и пароли (хеши) -> при входе выдаётся JWT с claims -> Authentication Middleware проверяет подпись, Issuer, Audience и срок и строит `ClaimsPrincipal` -> Authorization Middleware сверяет роли/политики -> контроллер/сервис проверяет владение ресурсом.

## Схема прохождения запроса

```
Client (Swagger / Postman / UniversityClient)
  | Authorization: Bearer <JWT>
  v
ExceptionHandlingMiddleware      (любые необработанные ошибки -> 500/409 в формате ReturnResult)
  v
Routing -> CORS (политика ClientApps)
  v
Authentication (JwtBearer)       подпись, Issuer, Audience, срок -> ClaimsPrincipal
  |  нет/плохой токен -> 401 (ReturnResult)
  v
Authorization                    [Authorize] / [AllowAnonymous] / роли и политики
  |  нет прав -> 403 (ReturnResult)
  v
Controller -> IResourceAccessService   владение ресурсом (свой курс / свои оценки) -> 403
  v
Service -> Repository -> EF Core -> PostgreSQL
```

## Версионирование API

| Маршрут | Версия | Ответ |
|---|---|---|
| `GET /api/courses`, `/api/v1/courses`, `/api/v1/courses/{id}` | v1 | `CourseDto` (контракт задания 1) |
| `GET /api/v2/courses`, `/api/v2/courses/{id}` | v2 | `CourseV2Dto`: + `teacher { id, fullName, department }`, `enrolledStudentsCount` |

Обе версии используют один `ICourseService` (фильтры и правила не дублируются), у v2 отдельные DTO. Версия v1 не менялась — проверка: `GET /api/v1/courses` возвращает тот же набор полей, что и раньше.

## Проверка сценариев

Файл `tests/scenarios.http` (VS Code REST Client / Rider) содержит все 12 сценариев раздела 7. Для сценария 5 (просроченный токен) запустите API с `Jwt__AccessTokenMinutes=1` и повторите запрос через минуту.
Примечание: в сценарии 6 таблицы задания указан 401, но по смыслу (пользователь аутентифицирован, прав нет) правильный код — 403, он и реализован. 401 возвращается только при отсутствующем/недействительном токене.

## Безопасность и логи

- Ключ подписи — только в user-secrets / переменных окружения; в `appsettings.json` его нет.
- Пароли хранятся хешами Identity; в ответах API нет `PasswordHash`, `SecurityStamp` и др. (`UserDto`).
- Пароли, JWT и ключ не логируются; при отклонении токена в лог пишется только тип ошибки.
- Блокировка: 5 неудачных попыток -> 10 минут. Пароль: 8+ символов, цифра, строчная и заглавная буквы.
- Роли и привязка записываются в токен при входе; после изменения ролей администратором новые права действуют с нового токена (срок жизни — `Jwt:AccessTokenMinutes`, по умолчанию 30 минут).
