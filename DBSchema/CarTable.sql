CREATE TABLE bilbasen_bil (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    owner_id INTEGER NOT NULL,
    FOREIGN KEY (owner_id) REFERENCES bilbasen_bruger(id)
);
