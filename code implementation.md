# Внедрение кода (Injection)

## Цель работы
Выявить наличие уязвимостей внедрения кода в веб-приложении, проанализировать возможность выполнения несанкционированных запросов к базе данных или исполнения произвольного кода на стороне сервера.

## Ход выполнения

### 1) Разведка

Для анализа был выбран модуль SQL Injection в DVWA:
<img width="1839" height="402" alt="image" src="https://github.com/user-attachments/assets/3efa7d49-d73c-482e-badf-2c6543688db6" />
Был перехвачен типичный запрос при вводе ID=1.

Данный модуль принимает идентификатор пользователя (User ID) через GET-параметр и возвращает соответствующую информацию из базы данных. В ответ сервер вернул информацию о пользователе с ID=1:

<img width="582" height="262" alt="image" src="https://github.com/user-attachments/assets/0e526146-efbc-48b7-88d0-fd81370c0ab0" />

Для успешного извлечения данных необходимо узнать, сколько столбцов возвращает исходный запрос. Для этого использовал команду:
1' ORDER BY 2-- -

text

<img width="575" height="273" alt="image" src="https://github.com/user-attachments/assets/f643b95e-fbbb-4ffc-ae24-036a04ea7e9a" />

Запрос выполнился успешно. Но уже при попытке сортировки по третьему столбцу появилась ошибка, что указывает на использование в запросе только двух столбцов:

<img width="1362" height="184" alt="image" src="https://github.com/user-attachments/assets/84052da0-9403-466d-9cb6-2a1ae0716702" />

Для понимания того, какие столбцы отображаются на странице, был введен UNION-запрос:
1' UNION SELECT 1,2-- -

text

<img width="628" height="365" alt="image" src="https://github.com/user-attachments/assets/0d026c5b-8416-4ffc-ba9b-d5c0389938ba" />

Цифры "1" и "2" отобразились на месте имени и фамилии. Значит обе позиции можно использовать для вывода данных.

### 2) Атака
Для получения списка всех таблиц в текущей базе данных был использован запрос, обращающийся к системной таблице information_schema, которая хранит метаданные о всех объектах базы данных:
1' UNION SELECT 1,group_concat(table_name) FROM information_schema.tables WHERE table_schema=database()-- -

text

<img width="1341" height="442" alt="image" src="https://github.com/user-attachments/assets/4696ae9f-5338-4091-a452-3fcbe68ef898" />

В ответе отобразились таблицы: guestbook, users. Таблица users представляет наибольший интерес, так как предположительно содержит учетные данные пользователей.

Для извлечения данных из таблицы users необходимо сначала узнать названия ее столбцов. Был использован запрос к information_schema.columns:
1' UNION SELECT 1,group_concat(column_name) FROM information_schema.columns WHERE table_name='users'-- -

text

<img width="1320" height="380" alt="image" src="https://github.com/user-attachments/assets/baad7131-d574-4449-af03-d8fd331ebb23" />

В ответе получены названия столбцов: user_id, first_name, last_name, user, password, avatar, last_login, failed_login. Наибольший интерес представляют столбцы user и password.

### 3) Эксплуатация
Зная названия интересующих столбцов, был составлен финальный запрос для извлечения учетных данных всех пользователей системы:
1' UNION SELECT user,password FROM users-- -

text

<img width="744" height="739" alt="image" src="https://github.com/user-attachments/assets/d7489ba9-709f-4def-bdfe-6d1bb55e2392" />

Полученные хеши паролей можно попытаться расшифровать. Для этого использован онлайн-сервис CrackStation. Хеш 8d3533d75ae2c3966d7e0d4fcc69216b был успешно расшифрован:

<img width="1954" height="770" alt="image" src="https://github.com/user-attachments/assets/653cdac8-6a04-49e9-9bac-9a4530f2708e" />

Результат расшифровки: пароль charley:

<img width="2124" height="812" alt="image" src="https://github.com/user-attachments/assets/87ec7995-6519-47b3-9040-1bd04836e518" />

Используя полученные данные (логин 1337, пароль charley), был произведен успешный вход в систему:

<img width="760" height="688" alt="image" src="https://github.com/user-attachments/assets/560a7853-0d34-4193-9040-cf7ad9d5f201" />

<img width="1446" height="306" alt="image" src="https://github.com/user-attachments/assets/4a2af049-bd1d-47d5-a5d8-13c554cd356f" />

## Выводы о защищенности
В результате анализа выявлена SQL-инъекция, позволяющая несанкционированно извлекать данные из базы. Отсутствие фильтрации входных данных в параметре id дает возможность модифицировать структуру SQL-запроса, что привело к получению учетных данных всех пользователей и успешному входу в систему. Эта возможность подтверждает критичность уязвимости.

Данная атака классифицируется как:

**CWE-89: Improper Neutralization of Special Elements used in an SQL Command** (SQL Injection)
