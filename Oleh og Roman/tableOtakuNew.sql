CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE,
    email VARCHAR(255) UNIQUE,
    password_hash TEXT,
    join_date DATE DEFAULT CURRENT_DATE,
    balance INTEGER DEFAULT 500 CHECK(balance >= 0), -- защита от ухода в минус
    role VARCHAR (15) DEFAULT 'user' CHECK (role in ('user', 'admin')) -- на будущее, можно будет пользователя
    -- наделять правами админа
);

CREATE TABLE avatars
    (
    avatar_id SERIAL PRIMARY KEY,
    avatar_name varchar(100) UNIQUE,
    rarity varchar(20) CHECK (rarity IN('common', 'rare', 'epic', 'legendary')),
    image_url TEXT,
    description TEXT
);
CREATE INDEX idx_avatars_rarity ON avatars(rarity);

CREATE TABLE weapons (
    weapon_id SERIAL PRIMARY KEY,
    weapon_name VARCHAR(100) UNIQUE,
    damage_min INTEGER CHECK ( damage_min >= 0 ),
    damage_max INTEGER CHECK ( damage_max >= damage_min),
    rarity VARCHAR(20) CHECK (rarity IN ('common', 'rare', 'epic', 'legendary')),
    image_url TEXT,
    description TEXT
);

CREATE TABLE avatar_inventory (
    inventory_id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(id) ON DELETE CASCADE,
    avatar_id INTEGER REFERENCES avatars(avatar_id),
    quantity INTEGER DEFAULT 1 CHECK (quantity >= 0),
    UNIQUE (user_id, avatar_id)
);

CREATE TABLE weapon_inventory (
    inventory_id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(id) ON DELETE CASCADE,
    weapon_id INTEGER REFERENCES weapons(weapon_id),
    quantity INTEGER DEFAULT 1 CHECK (quantity >= 0),
    UNIQUE (user_id, weapon_id)
);

CREATE TABLE shop_items (
    shop_item_id SERIAL PRIMARY KEY,
    item_type VARCHAR(10) CHECK (item_type IN ('Avatar', 'Weapon')),
    item_id INTEGER NOT NULL,
    price INTEGER CHECK(price >= 0),
    quantity INTEGER default 1 CHECK ( quantity >= 0 )
);

CREATE table trades (
    trade_id SERIAL PRIMARY KEY, -- id самой сделки
    seller_id INTEGER REFERENCES users(id) ON DELETE SET NULL, --после удаления пользователя, оружие не удалится,
    -- а в его поле "владелец" будет просто NULL
    buyer_id INTEGER REFERENCES users(id) ON DELETE SET NULL,
    item_type VARCHAR(10) CHECK ( item_type IN ('Avatar', 'Weapon')),
    item_id INTEGER NOT NULL, -- id нашего итема, может быть или аватар или виапон
    price INTEGER CHECK(price >= 0),
    trade_date timestamp default CURRENT_TIMESTAMP
);

--DROP TABLE IF EXISTS inventory;
--DROP TABLE IF EXISTS weapons;
--DROP TABLE IF EXISTS avatars;
--DROP TABLE IF EXISTS users;


