-- Создание схемы
CREATE SCHEMA IF NOT EXISTS kp;
SET search_path TO kp;

-- =====================================================
-- 1. СОЗДАНИЕ ТАБЛИЦ
-- =====================================================

-- Статусы заявок
CREATE TABLE IF NOT EXISTS kp.request_statuses (
    id SERIAL PRIMARY KEY,
    status_name VARCHAR(50) NOT NULL UNIQUE
);

-- Цели посещения
CREATE TABLE IF NOT EXISTS kp.visit_purposes (
    id SERIAL PRIMARY KEY,
    purpose_name VARCHAR(255) NOT NULL UNIQUE
);

-- Подразделения
CREATE TABLE IF NOT EXISTS kp.departments (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL UNIQUE
);

-- Сотрудники подразделений
CREATE TABLE IF NOT EXISTS kp.employees (
    id SERIAL PRIMARY KEY,
    department_id INT NOT NULL REFERENCES kp.departments(id) ON DELETE CASCADE,
    full_name VARCHAR(255) NOT NULL,
    position VARCHAR(255)
);

-- Пользователи (гости)
CREATE TABLE IF NOT EXISTS kp.users (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Личные заявки
CREATE TABLE IF NOT EXISTS kp.personal_requests (
    id SERIAL PRIMARY KEY,
    user_id INT NOT NULL REFERENCES kp.users(id) ON DELETE CASCADE,
    status_id INT NOT NULL REFERENCES kp.request_statuses(id),
    purpose_id INT NOT NULL REFERENCES kp.visit_purposes(id),
    department_id INT NOT NULL REFERENCES kp.departments(id),
    employee_id INT NOT NULL REFERENCES kp.employees(id),
    
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    visit_comment TEXT NOT NULL,
    
    last_name VARCHAR(100) NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    middle_name VARCHAR(100),
    phone VARCHAR(20),
    email_visitor VARCHAR(255) NOT NULL,
    organization VARCHAR(255),
    birth_date DATE NOT NULL,
    passport_series VARCHAR(4) NOT NULL,
    passport_number VARCHAR(6) NOT NULL,
    photo_path TEXT,
    passport_scan_path TEXT NOT NULL,
    
    rejection_reason TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CHECK (start_date >= CURRENT_DATE + 1),
    CHECK (start_date <= CURRENT_DATE + 15),
    CHECK (end_date >= start_date),
    CHECK (end_date <= start_date + 15),
    CHECK (birth_date <= CURRENT_DATE - INTERVAL '16 years')
);

-- Групповые заявки
CREATE TABLE IF NOT EXISTS kp.group_requests (
    id SERIAL PRIMARY KEY,
    user_id INT NOT NULL REFERENCES kp.users(id) ON DELETE CASCADE,
    status_id INT NOT NULL REFERENCES kp.request_statuses(id),
    purpose_id INT NOT NULL REFERENCES kp.visit_purposes(id),
    department_id INT NOT NULL REFERENCES kp.departments(id),
    employee_id INT NOT NULL REFERENCES kp.employees(id),
    
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    visit_comment TEXT NOT NULL,
    
    group_leader_last_name VARCHAR(100) NOT NULL,
    group_leader_first_name VARCHAR(100) NOT NULL,
    group_leader_middle_name VARCHAR(100),
    group_leader_phone VARCHAR(20),
    group_leader_email VARCHAR(255) NOT NULL,
    group_leader_organization VARCHAR(255),
    group_leader_birth_date DATE NOT NULL,
    group_leader_passport_series VARCHAR(4) NOT NULL,
    group_leader_passport_number VARCHAR(6) NOT NULL,
    passport_scan_path TEXT NOT NULL,
    
    rejection_reason TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CHECK (start_date >= CURRENT_DATE + 1),
    CHECK (start_date <= CURRENT_DATE + 15),
    CHECK (end_date >= start_date),
    CHECK (end_date <= start_date + 15),
    CHECK (group_leader_birth_date <= CURRENT_DATE - INTERVAL '16 years')
);

-- Члены группы
CREATE TABLE IF NOT EXISTS kp.group_members (
    id SERIAL PRIMARY KEY,
    group_request_id INT NOT NULL REFERENCES kp.group_requests(id) ON DELETE CASCADE,
    row_number INT NOT NULL,
    full_name_initials VARCHAR(255) NOT NULL,
    contact_info TEXT NOT NULL,
    photo_path TEXT
);

-- =====================================================
-- 2. ЗАПОЛНЕНИЕ СПРАВОЧНИКОВ
-- =====================================================

INSERT INTO kp.request_statuses (status_name) VALUES 
('проверка'), 
('одобрена'), 
('не одобрена')
ON CONFLICT (status_name) DO NOTHING;

INSERT INTO kp.visit_purposes (purpose_name) VALUES 
('Рабочая встреча'), 
('Экскурсия'), 
('Техническое обслуживание')
ON CONFLICT (purpose_name) DO NOTHING;

INSERT INTO kp.departments (name) VALUES 
('IT-отдел'), 
('Отдел безопасности'), 
('Бухгалтерия'),
('Отдел кадров'),
('Производственный отдел')
ON CONFLICT (name) DO NOTHING;

INSERT INTO kp.employees (department_id, full_name, position) VALUES
(1, 'Иванов Иван Иванович', 'Начальник IT'),
(1, 'Петров Петр Петрович', 'Инженер'),
(2, 'Сидоров Сидор Сидорович', 'Начальник охраны'),
(3, 'Кузнецова Мария Ивановна', 'Главный бухгалтер'),
(4, 'Степанова Радинка Власовна', 'Специалист по кадрам'),
(5, 'Шилов Прохор Герасимович', 'Начальник производства')
ON CONFLICT DO NOTHING;

-- =====================================================
-- 3. ИМПОРТ ПОЛЬЗОВАТЕЛЕЙ ИЗ ТАБЛИЦЫ
-- =====================================================

INSERT INTO kp.users (email, password_hash) VALUES
('Radinka100@yandex.ru', MD5('b3uWS6#Thuvq')),
('Prohor156@list.ru', MD5('zDdom}SIhWs?')),
('YUrin155@gmail.com', MD5('u@m*~ACBCqNQ')),
('Aljbina33@lenta.ru', MD5('Bu?BHCtwDFin')),
('Klavdiya113@live.com', MD5('FjC#hNIJori}')),
('Tamara179@live.com', MD5('TJxVqMXrbesI')),
('Taras24@rambler.ru', MD5('07m5yspn3K~K')),
('Arkadij123@inbox.ru', MD5('vk2N7lxX}ck%')),
('Glafira73@outlook.com', MD5('Zz8POQlP}M4~')),
('Gavriila68@msn.com', MD5('x4K5WthEe8ua')),
('Kuzjma124@yandex.ru', MD5('OsByQJ}vYznW')),
('Roman89@gmail.com', MD5('Xd?xP$2yICcG')),
('Aleksej43@gmail.com', MD5('~c%PlTY0?qgl')),
('Nadezhda137@outlook.com', MD5('QQ~0q~rXHb?p')),
('Bronislava56@yahoo.com', MD5('LO}xyC~1S4l6'))
ON CONFLICT (email) DO NOTHING;

-- =====================================================
-- 4. ФУНКЦИЯ ДЛЯ ПАРСИНГА ДАТЫ ИЗ ФОРМАТА DD/MM/YYYY
-- =====================================================

CREATE OR REPLACE FUNCTION kp.parse_date_from_string(date_str TEXT)
RETURNS DATE AS $$
DECLARE
    day INT;
    month INT;
    year INT;
    parts TEXT[];
BEGIN
    -- Извлекаем дату из строки формата "24/04/2023_9367788"
    date_str := split_part(date_str, '_', 1);
    
    -- Разбиваем по символу '/'
    parts := string_to_array(date_str, '/');
    
    IF array_length(parts, 1) = 3 THEN
        day := parts[1]::INT;
        month := parts[2]::INT;
        year := parts[3]::INT;
        RETURN MAKE_DATE(year, month, day);
    ELSE
        -- Если не удалось распарсить, возвращаем текущую дату + 1 день
        RETURN CURRENT_DATE + 1;
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RETURN CURRENT_DATE + 1;
END;
$$ LANGUAGE plpgsql IMMUTABLE;

-- =====================================================
-- 5. ФУНКЦИЯ ДЛЯ ПАРСИНГА РУССКОЙ ДАТЫ РОЖДЕНИЯ
-- =====================================================

CREATE OR REPLACE FUNCTION kp.parse_russian_birthdate(date_str TEXT)
RETURNS DATE AS $$
DECLARE
    day INT;
    month INT;
    year INT;
    month_name TEXT;
    month_map TEXT[];
    i INT;
BEGIN
    -- Пример: "18 октября 1986 года"
    -- Извлекаем день
    day := split_part(date_str, ' ', 1)::INT;
    
    -- Извлекаем месяц
    month_name := split_part(date_str, ' ', 2);
    
    -- Извлекаем год
    year := split_part(split_part(date_str, ' ', 3), ' ', 1)::INT;
    
    -- Массив названий месяцев
    month_map := ARRAY['января','февраля','марта','апреля','мая','июня',
                       'июля','августа','сентября','октября','ноября','декабря'];
    
    -- Находим номер месяца
    month := 1;
    FOR i IN 1..12 LOOP
        IF month_name = month_map[i] THEN
            month := i;
            EXIT;
        END IF;
    END LOOP;
    
    RETURN MAKE_DATE(year, month, day);
EXCEPTION
    WHEN OTHERS THEN
        -- В случае ошибки возвращаем дату, чтобы не блокировать вставку
        RETURN '1990-01-01'::DATE;
END;
$$ LANGUAGE plpgsql IMMUTABLE;

-- =====================================================
-- 6. ИМПОРТ ЛИЧНЫХ ЗАЯВОК
-- =====================================================

INSERT INTO kp.personal_requests (
    user_id, status_id, purpose_id, department_id, employee_id,
    start_date, end_date, visit_comment,
    last_name, first_name, middle_name, phone, email_visitor, organization,
    birth_date, passport_series, passport_number, passport_scan_path
)
SELECT 
    u.id AS user_id,
    (SELECT id FROM kp.request_statuses WHERE status_name = 'проверка') AS status_id,
    (SELECT id FROM kp.visit_purposes WHERE purpose_name = 'Рабочая встреча') AS purpose_id,
    d.id AS department_id,
    e.id AS employee_id,
    kp.parse_date_from_string(import_data."Назначение") AS start_date,
    kp.parse_date_from_string(import_data."Назначение") + INTERVAL '1 day' AS end_date,
    'Посещение по рабочему вопросу' AS visit_comment,
    split_part(import_data."ФИО", ' ', 1) AS last_name,
    split_part(import_data."ФИО", ' ', 2) AS first_name,
    split_part(import_data."ФИО", ' ', 3) AS middle_name,
    import_data."Номер телефона" AS phone,
    import_data."E-mail" AS email_visitor,
    CASE 
        WHEN import_data."Логин" LIKE '%86' THEN 'ООО ТехноСервис'
        WHEN import_data."Логин" LIKE '%55' THEN 'ЗАО ПромСтрой'
        WHEN import_data."Логин" LIKE '%24' THEN 'ООО Инновации'
        ELSE 'Не указано'
    END AS organization,
    kp.parse_russian_birthdate(import_data."Дата рождения") AS birth_date,
    split_part(import_data."Данные паспорта", ' ', 1) AS passport_series,
    split_part(import_data."Данные паспорта", ' ', 2) AS passport_number,
    '/scans/' || import_data."Логин" || '_passport.pdf' AS passport_scan_path
FROM (
    VALUES 
        ('Степанова Радинка Власовна', '+7 (613) 272-60-62', 'Radinka100@yandex.ru', '18 октября 1986 года', '0208 530509', 'Vlas86', 'b3uWS6#Thuvq', '24/04/2023_9367788'),
        ('Шилов Прохор Герасимович', '+7 (615) 594-77-66', 'Prohor156@list.ru', '9 октября 1977 года', '3036 796488', 'Prohor156', 'zDdom}SIhWs?', '24/04/2023_9788737'),
        ('Кононов Юрин Романович', '+7 (784) 673-51-91', 'YUrin155@gmail.com', '8 октября 1971 года', '2747 790512', 'YUrin155', 'u@m*~ACBCqNQ', '24/04/2023_9736379'),
        ('Елисеева Альбина Николаевна', '+7 (654) 864-77-46', 'Aljbina33@lenta.ru', '15 февраля 1983 года', '5241 213304', 'Aljbina33', 'Bu?BHCtwDFin', '25/04/2023_9367788'),
        ('Шарова Клавдия Макаровна', '+7 (822) 525-82-40', 'Klavdiya113@live.com', '22 июля 1980 года', '8143 593309', 'Klavdiya113', 'FjC#hNIJori}', '25/04/2023_9788737'),
        ('Сидорова Тамара Григорьевна', '+7 (334) 692-79-77', 'Tamara179@live.com', '22 ноября 1995 года', '8143 905520', 'Tamara179', 'TJxVqMXrbesI', '25/04/2023_9736379'),
        ('Петухов Тарас Фадеевич', '+7 (376) 220-62-51', 'Taras24@rambler.ru', '5 января 1991 года', '1609 171096', 'Taras24', '07m5yspn3K~K', '26/04/2023_9367788'),
        ('Родионов Аркадий Власович', '+7 (491) 696-17-11', 'Arkadij123@inbox.ru', '11 августа 1993 года', '3841 642594', 'Arkadij123', 'vk2N7lxX}ck%', '26/04/2023_9788737'),
        ('Горшкова Глафира Валентиновна', '+7 (553) 343-38-82', 'Glafira73@outlook.com', '25 мая 1978 года', '9170 402601', 'Glafira73', 'Zz8POQlP}M4~', '26/04/2023_9736379'),
        ('Кириллова Гавриила Яковна', '+7 (648) 700-43-34', 'Gavriila68@msn.com', '26 апреля 1992 года', '9438 379667', 'Gavriila68', 'x4K5WthEe8ua', '27/04/2023_9367788'),
        ('Овчинников Кузьма Ефимович', '+7 (562) 866-15-27', 'Kuzjma124@yandex.ru', '2 августа 1993 года', '0766 647226', 'Kuzjma124', 'OsByQJ}vYznW', '27/04/2023_9788737'),
        ('Беляков Роман Викторович', '+7 (595) 196-56-28', 'Roman89@gmail.com', '7 июня 1991 года', '2411 478305', 'Roman89', 'Xd?xP$2yICcG', '27/04/2023_9736379'),
        ('Лыткин Алексей Максимович', '+7 (994) 353-29-52', 'Aleksej43@gmail.com', '7 марта 1996 года', '2383 259825', 'Aleksej43', '~c%PlTY0?qgl', '28/04/2023_9367788'),
        ('Шубина Надежда Викторовна', '+7 (736) 488-66-95', 'Nadezhda137@outlook.com', '24 сентября 1981 года', '8844 708476', 'Nadezhda137', 'QQ~0q~rXHb?p', '28/04/2023_9788737'),
        ('Зиновьева Бронислава Викторовна', '+7 (778) 565-12-18', 'Bronislava56@yahoo.com', '19 марта 1981 года', '6736 319423', 'Bronislava56', 'LO}xyC~1S4l6', '28/04/2023_9736379')
) AS import_data("ФИО", "Номер телефона", "E-mail", "Дата рождения", "Данные паспорта", "Логин", "Пароль", "Назначение")
JOIN kp.users u ON u.email = import_data."E-mail"
CROSS JOIN LATERAL (
    SELECT id FROM kp.departments ORDER BY RANDOM() LIMIT 1
) d
CROSS JOIN LATERAL (
    SELECT id FROM kp.employees WHERE department_id = d.id ORDER BY RANDOM() LIMIT 1
) e
WHERE kp.parse_date_from_string(import_data."Назначение") >= CURRENT_DATE + 1
  AND kp.parse_date_from_string(import_data."Назначение") <= CURRENT_DATE + 15
ON CONFLICT DO NOTHING;

-- =====================================================
-- 7. СОЗДАНИЕ ИНДЕКСОВ
-- =====================================================

CREATE INDEX IF NOT EXISTS idx_personal_user_id ON kp.personal_requests(user_id);
CREATE INDEX IF NOT EXISTS idx_personal_status ON kp.personal_requests(status_id);
CREATE INDEX IF NOT EXISTS idx_personal_dates ON kp.personal_requests(start_date, end_date);
CREATE INDEX IF NOT EXISTS idx_group_user_id ON kp.group_requests(user_id);
CREATE INDEX IF NOT EXISTS idx_group_status ON kp.group_requests(status_id);
CREATE INDEX IF NOT EXISTS idx_group_dates ON kp.group_requests(start_date, end_date);
CREATE INDEX IF NOT EXISTS idx_group_members_request ON kp.group_members(group_request_id);
CREATE INDEX IF NOT EXISTS idx_users_email ON kp.users(email);

-- =====================================================
-- 8. ФУНКЦИЯ ПРОВЕРКИ ПАРОЛЯ
-- =====================================================

CREATE OR REPLACE FUNCTION kp.check_user_password(
    p_email VARCHAR,
    p_password VARCHAR
) RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (
        SELECT 1 FROM kp.users
        WHERE email = p_email
          AND password_hash = MD5(p_password)
    );
END;
$$ LANGUAGE plpgsql STABLE;

-- =====================================================
-- 9. ТЕСТОВЫЕ ДАННЫЕ ДЛЯ ГРУППОВЫХ ЗАЯВОК
-- =====================================================

INSERT INTO kp.group_requests (
    user_id, status_id, purpose_id, department_id, employee_id,
    start_date, end_date, visit_comment,
    group_leader_last_name, group_leader_first_name, group_leader_middle_name,
    group_leader_phone, group_leader_email, group_leader_organization,
    group_leader_birth_date, group_leader_passport_series, group_leader_passport_number,
    passport_scan_path
)
SELECT
    u.id,
    (SELECT id FROM kp.request_statuses WHERE status_name = 'проверка'),
    (SELECT id FROM kp.visit_purposes WHERE purpose_name = 'Экскурсия'),
    d.id,
    e.id,
    CURRENT_DATE + 5,
    CURRENT_DATE + 6,
    'Групповая экскурсия по предприятию',
    'Иванов', 'Петр', 'Сидорович',
    '+7 (999) 111-22-33',
    u.email,
    'ООО Экскурсионное бюро',
    '1985-06-15',
    '1234', '567890',
    '/scans/group_leader_scan.pdf'
FROM kp.users u
CROSS JOIN LATERAL (SELECT id FROM kp.departments ORDER BY RANDOM() LIMIT 1) d
CROSS JOIN LATERAL (SELECT id FROM kp.employees WHERE department_id = d.id ORDER BY RANDOM() LIMIT 1) e
WHERE u.id IN (1, 2, 3)
LIMIT 2
ON CONFLICT DO NOTHING;

-- Члены групп для созданных групповых заявок
INSERT INTO kp.group_members (group_request_id, row_number, full_name_initials, contact_info)
SELECT 
    gr.id,
    seq.n,
    CASE seq.n
        WHEN 1 THEN 'Петров П.П.'
        WHEN 2 THEN 'Сидоров С.С.'
        WHEN 3 THEN 'Кузнецова М.И.'
        WHEN 4 THEN 'Васильев В.В.'
        WHEN 5 THEN 'Павлова А.Н.'
    END,
    CASE seq.n
        WHEN 1 THEN 'тел. +7 (999) 111-22-33, email: petrov@example.com'
        WHEN 2 THEN 'тел. +7 (999) 444-55-66, email: sidorov@example.com'
        WHEN 3 THEN 'тел. +7 (999) 777-88-99, email: kuznecova@example.com'
        WHEN 4 THEN 'тел. +7 (999) 000-11-22, email: vasiliev@example.com'
        WHEN 5 THEN 'тел. +7 (999) 333-44-55, email: pavlova@example.com'
    END
FROM kp.group_requests gr
CROSS JOIN LATERAL generate_series(1, 5) AS seq(n)
WHERE gr.id IN (SELECT id FROM kp.group_requests LIMIT 2)
ON CONFLICT DO NOTHING;

-- =====================================================
-- 10. ВЫВОД ИНФОРМАЦИИ
-- =====================================================

DO $$
DECLARE
    user_count INT;
    personal_count INT;
    group_count INT;
    personal_dates TEXT;
BEGIN
    SELECT COUNT(*) INTO user_count FROM kp.users;
    SELECT COUNT(*) INTO personal_count FROM kp.personal_requests;
    SELECT COUNT(*) INTO group_count FROM kp.group_requests;
    
    SELECT STRING_AGG(DISTINCT start_date::TEXT, ', ') INTO personal_dates 
    FROM kp.personal_requests LIMIT 5;
    
    RAISE NOTICE '=== ИМПОРТ ЗАВЕРШЕН ===';
    RAISE NOTICE 'Пользователей: %', user_count;
    RAISE NOTICE 'Личных заявок: %', personal_count;
    RAISE NOTICE 'Групповых заявок: %', group_count;
    RAISE NOTICE '========================';
END;
$$;