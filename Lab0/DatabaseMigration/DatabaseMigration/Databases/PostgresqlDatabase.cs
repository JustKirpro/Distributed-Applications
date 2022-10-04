using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using DatabaseMigration.Data_structures;
using Npgsql;

namespace DatabaseMigration.Databases
{
    public static class PostgresqlDatabase
    {
        private static readonly string BaseConnectionString = new NpgsqlConnectionStringBuilder()
        {
            Host = ConfigurationManager.AppSettings.Get("PostgresHost"),
            Port = Convert.ToInt32(ConfigurationManager.AppSettings.Get("PostgresPort")),
            Username = ConfigurationManager.AppSettings.Get("PostgresUsername"),
            Password = ConfigurationManager.AppSettings.Get("PostgresPassword")
        }.ConnectionString;

        private static readonly string DatabaseName = ConfigurationManager.AppSettings.Get("PostgresDatabase");

        /// <summary>
        /// <exception cref="NpgsqlException"> Thrown when the PostgreSQL database problem has occured. </exception>
        /// </summary>
        public static void CreateDatabaseSchema()
        {
            CreateDatabase();
            CreateSchema();
        }

        /// <summary>
        /// <exception cref="NpgsqlException"> Thrown when the database problem has occured. </exception>
        /// <exception cref="IOException"> Thrown when the file with SQL script is not found. </exception>
        /// </summary>
        public static void FillDatabase(List<Record> records)
        {
            var connectionString = BaseConnectionString + $";Database={DatabaseName};";

            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            foreach (var record in records) 
            {
                var userId = InsertUser(connection, record);
                var playlistId = InsertPlaylist(connection, record, userId);
                var artistId = InsertArtist(connection, record);
                var albumId = InsertAlbum(connection, record, artistId);
                var songId = InsertSong(connection, record, albumId);
                InsertPlaylistSong(connection, record, playlistId, songId);
            }
        }

        /// <summary>
        /// <exception cref="NpgsqlException"> Thrown when the PostgreSQL database problem has occured. </exception>
        /// </summary>
        public static List<Table> ReadPlaylistSongTable()
        {
            var connectionString = BaseConnectionString + $";Database={DatabaseName};";
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            
            var logins = ReadLogins(connection);
            var data = new List<Table>();

            const string commandText = @"SELECT login, p.name AS playlist_name, p.description AS playlist_description, ar.name AS artist_name,
                                            ar.description AS artist_description, al.name AS album_name, rating AS album_rating,
                                            s.name AS song_name, duration AS song_duration, plays_number
                                         FROM ""user"" JOIN playlist AS p USING (user_id) JOIN playlist_song USING (playlist_id) JOIN song AS s USING (song_id)
                                            JOIN album AS al USING (album_id) JOIN artist AS ar USING (artist_id)
                                         WHERE
                                            login = @login";

            foreach (var login in logins)
            {
                var table = new Table
                {
                    Name = login,
                    Records = new List<Dictionary<string, object>>()
                };
                
                var sqlCommand = new NpgsqlCommand(commandText, connection);
                sqlCommand.Parameters.AddWithValue("login", login);
                
                                
                using var reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    var record = new Dictionary<string, object>();
            
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var columnName = reader.GetName(i);
                        var value = reader[columnName];
                        record.Add(columnName, value);
                    }
                    
                    table.Records.Add(record);
                }
            
                data.Add(table);
            }
            
            return data;
        }

        private static List<string> ReadLogins(NpgsqlConnection connection)
        { 
            const string commandText = @"SELECT login FROM ""user""";
            var sqlCommand = new NpgsqlCommand(commandText, connection);

            List<string> logins = new();
            using var reader = sqlCommand.ExecuteReader();
            while (reader.Read())
            {
                logins.Add((string) reader["login"]);
            }

            return logins;
        }
        
        private static void CreateDatabase()
        {
            using var connection = new NpgsqlConnection(BaseConnectionString);
            connection.Open();

            var commandText = $"DROP DATABASE IF EXISTS {DatabaseName} WITH (FORCE); CREATE DATABASE {DatabaseName};";
            var sqlCommand = new NpgsqlCommand(commandText, connection);
            sqlCommand.ExecuteNonQuery();
        }

        private static void CreateSchema()
        {
            var path = ConfigurationManager.AppSettings.Get("SQLScriptPath");
            var commandText = File.ReadAllText(path);

            var connectionString = BaseConnectionString + $";Database={DatabaseName};";
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            var sqlCommand = new NpgsqlCommand(commandText, connection);
            sqlCommand.ExecuteNonQuery();
        }
        
        private static int InsertEntity(NpgsqlConnection connection, string selectCommandText, string insertCommandText, Dictionary<string, object> parameters)
        {
            var sqlCommand = new NpgsqlCommand(selectCommandText, connection);

            foreach (var entity in parameters)
            {
                sqlCommand.Parameters.AddWithValue(entity.Key, entity.Value);
            }

            var entityId = (int?) sqlCommand.ExecuteScalar();

            if (entityId is not null)
                return entityId.Value;
            
            sqlCommand.CommandText = insertCommandText;
            entityId = (int?) sqlCommand.ExecuteScalar();
            return entityId.Value;
        }

        private static int InsertUser(NpgsqlConnection connection, Record record)
        {
            const string selectCommandText = @"SELECT user_id FROM ""user"" WHERE login = @login AND password = @password";
            const string insertCommandText = @"INSERT INTO ""user"" (login, password) VALUES (@login, @password) RETURNING user_id";
            var parameters = new Dictionary<string, object>
            {
                ["login"] = record.UserLogin,
                ["password"] = record.UserPassword
            };
            
            return InsertEntity(connection, selectCommandText, insertCommandText, parameters);
        }

        private static int InsertPlaylist(NpgsqlConnection connection, Record record, int userId)
        {
            const string selectCommandText = @"SELECT playlist_id FROM playlist WHERE name = @name AND description = @description AND user_id = @user_id";
            const string insertCommandText = @"INSERT INTO playlist (name, description, user_id) VALUES (@name, @description, @user_id) RETURNING playlist_id";
            var parameters = new Dictionary<string, object>
            {
                ["name"] = record.PlaylistName,
                ["description"] = record.PlaylistDescription,
                ["user_id"] = userId
            };

            return InsertEntity(connection, selectCommandText, insertCommandText, parameters);
        }
        
        private static int InsertArtist(NpgsqlConnection connection, Record record)
        {
            const string selectCommandText = @"SELECT artist_id FROM artist WHERE name = @name AND description = @description";
            const string insertCommandText = @"INSERT INTO artist (name, description) VALUES (@name, @description) RETURNING artist_id";
            var parameters = new Dictionary<string, object>
            {
                ["name"] = record.ArtistName,
                ["description"] = record.ArtistDescription
            };

            return InsertEntity(connection, selectCommandText, insertCommandText, parameters);
        }
        
        private static int InsertAlbum(NpgsqlConnection connection, Record record, int artistId)
        {
            const string selectCommandText = @"SELECT album_id FROM album WHERE name = @name AND rating = @rating AND artist_id = @artist_id";
            const string insertCommandText = @"INSERT INTO album (name, rating, artist_id) VALUES (@name, @rating, @artist_id) RETURNING album_id";
            var parameters = new Dictionary<string, object>
            {
                ["name"] = record.AlbumName,
                ["rating"] = record.AlbumRating,
                ["artist_id"] = artistId
            };

            return InsertEntity(connection, selectCommandText, insertCommandText, parameters);
        }
        
        private static int InsertSong(NpgsqlConnection connection, Record record, int albumId)
        {
            const string selectCommandText = @"SELECT song_id FROM song WHERE name = @name AND duration = @duration AND album_id = @album_id";
            const string insertCommandText = @"INSERT INTO song (name, duration, album_id) VALUES (@name, @duration, @album_id) RETURNING song_id";
            var parameters = new Dictionary<string, object>
            {
                ["name"] = record.SongName,
                ["duration"] = record.SongDuration,
                ["album_id"] = albumId
            };

            return InsertEntity(connection, selectCommandText, insertCommandText, parameters);
        }
        
        private static void InsertPlaylistSong(NpgsqlConnection connection, Record record, int playlistId, int songId)
        {
            const string commandText = @"SELECT plays_number FROM playlist_song WHERE playlist_id = @playlist_id AND song_id = @song_id";
            var sqlCommand = new NpgsqlCommand(commandText, connection);
            sqlCommand.Parameters.AddWithValue("playlist_id", playlistId);
            sqlCommand.Parameters.AddWithValue("song_id", songId);
            var playsNumber = (int?) sqlCommand.ExecuteScalar();

            if (playsNumber is null)
            {
                sqlCommand.CommandText = @"INSERT INTO playlist_song (playlist_id, song_id, plays_number) VALUES (@playlist_id, @song_id, @plays_number)";
                sqlCommand.Parameters.AddWithValue("plays_number", record.PlaysNumber);
            }
            else
            {
                sqlCommand.CommandText = @"UPDATE playlist_song SET plays_number = @plays_number WHERE playlist_id = @playlist_id AND song_id = @song_id";
                sqlCommand.Parameters.AddWithValue("plays_number", playsNumber.Value + record.PlaysNumber);
            }
            
            sqlCommand.ExecuteNonQuery();
        }
    }
}