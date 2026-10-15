-- Base de aprendizagem da aula 5.
-- Corre este ficheiro sempre que quiseres recomeçar do zero: apaga e volta a criar as tabelas.

DROP TABLE IF EXISTS comments;
DROP TABLE IF EXISTS tickets;

CREATE TABLE tickets (
    id          integer     GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    title       text        NOT NULL CHECK (char_length(btrim(title)) BETWEEN 5 AND 120),
    description text        NOT NULL DEFAULT '' CHECK (char_length(description) <= 2000),
    status      text        NOT NULL DEFAULT 'Open'
                            CHECK (status IN ('Open', 'InProgress', 'Resolved')),
    priority    smallint    NOT NULL DEFAULT 2 CHECK (priority BETWEEN 1 AND 3),
    created_at  timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE comments (
    id          integer     GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ticket_id   integer     NOT NULL REFERENCES tickets (id),
    author      text        NOT NULL CHECK (char_length(btrim(author)) BETWEEN 1 AND 80),
    body        text        NOT NULL CHECK (char_length(btrim(body)) BETWEEN 1 AND 2000),
    created_at  timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX comments_ticket_id_idx ON comments (ticket_id);
