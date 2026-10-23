-- Роли ASP.NET Core Identity. Выполнять после создания схемы (после первого запуска API или применения миграций).
-- Приложение и так создаёт эти роли при старте (IdentitySeeder); скрипт нужен для ручной подготовки БД.
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
SELECT gen_random_uuid()::text, r.name, upper(r.name), gen_random_uuid()::text
FROM (VALUES ('Admin'), ('Teacher'), ('Student')) AS r(name)
WHERE NOT EXISTS (SELECT 1 FROM "AspNetRoles" x WHERE x."NormalizedName" = upper(r.name));

-- Пользователей напрямую SQL-ом не создаём: хеш пароля формирует только Identity.
-- Тестовые пользователи (admin, teacher.gertsen, teacher.abdoldina, student.alex, student.aigerim)
-- создаются IdentitySeeder при запуске в Development (пароль — Seed:Password из appsettings.Development.json).
