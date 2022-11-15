CREATE TABLE "user"
(
    user_id SERIAL CONSTRAINT user_pk PRIMARY KEY,
    login TEXT NOT NULL,
    password TEXT NOT NULL
);

CREATE TABLE playlist
(
    playlist_id SERIAL CONSTRAINT playlist_pk PRIMARY KEY,
    name TEXT NOT NULL,
    description TEXT,
    user_id INTEGER NOT NULL CONSTRAINT playlist_user_fk REFERENCES "user"
);

CREATE TABLE artist
(
    artist_id SERIAL CONSTRAINT artist_pk PRIMARY KEY,
    name TEXT NOT NULL,
    description TEXT NOT NULL
);

CREATE TABLE album
(
    album_id SERIAL CONSTRAINT album_pk PRIMARY KEY,
    name TEXT NOT NULL,
    rating INTEGER,
    artist_id INTEGER NOT NULL CONSTRAINT album_artist_fk REFERENCES artist
);

CREATE TABLE song
(
    song_id SERIAL CONSTRAINT song_pk PRIMARY KEY,
    name TEXT NOT NULL,
    duration INTEGER,
    album_id INTEGER NOT NULL CONSTRAINT song_album_fk REFERENCES album
);

CREATE TABLE playlist_song
(
    playlist_id INTEGER NOT NULL constraint playlist_song_fk REFERENCES playlist,
    song_id INTEGER NOT NULL CONSTRAINT playlist_song_song_fk REFERENCES song,
    plays_number integer,
    CONSTRAINT playlist_song_pk PRIMARY KEY (playlist_id, song_id)
);

CREATE VIEW test_view AS
SELECT login, password, p.name AS playlist_name, p.description AS playlist_description, ar.name AS artist_name, 
    ar.description AS artist_description, al.name AS album_name, al.rating AS album_rating, s.name AS song_name,
    s.duration AS song_duration, plays_number
FROM "user" JOIN playlist AS p USING (user_id) JOIN playlist_song USING (playlist_id)JOIN song AS s USING (song_id) 
    JOIN album AS al USING (album_id) JOIN artist AS ar USING(artist_id);