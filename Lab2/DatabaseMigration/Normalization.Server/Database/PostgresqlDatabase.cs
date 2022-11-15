using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using Npgsql;

namespace Normalization.Server.Database;

public static class PostgresqlDatabase
{
    private static readonly string BaseConnectionString = ConfigurationManager.AppSettings.Get("PostgresConnectionString");
    private static readonly string DatabaseName = ConfigurationManager.AppSettings.Get("PostgresDatabase");
    
    /// <summary>
    /// <exception cref="NpgsqlException"> PostgreSQL database problem has occured. </exception>
    /// </summary>
    public static void InsertRecord(Record record)
    {
        var connectionString = BaseConnectionString + $";Database={DatabaseName};";
    
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);

        try
        {
            var userId = InsertUser(connection, record, transaction);
            var playlistId = InsertPlaylist(connection, record, userId, transaction);
            var artistId = InsertArtist(connection, record, transaction);
            var albumId = InsertAlbum(connection, record, artistId, transaction);
            var songId = InsertSong(connection, record, albumId, transaction);
            InsertPlaylistSong(connection, record, playlistId, songId, transaction);
        }
        catch (NpgsqlException)
        {
            transaction.Rollback();
            throw;
        }

        transaction.Commit();
    }
    
    /// <summary>
    /// <exception cref="NpgsqlException"> PostgreSQL database problem has occured. </exception>
    /// <exception cref="IOException"> SQL script file was not found. </exception>
    /// </summary>
    public static void CreateDatabaseSchema()
    {
        CreateDatabase();
        CreateSchema();
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
        var scriptPath = ConfigurationManager.AppSettings.Get("SqlScriptPath");
        var commandText = File.ReadAllText(scriptPath);

        var connectionString = BaseConnectionString + $";Database={DatabaseName};";
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        var sqlCommand = new NpgsqlCommand(commandText, connection);
        sqlCommand.ExecuteNonQuery();
    }
        
    private static int InsertEntity(NpgsqlConnection connection, string selectCommandText, string insertCommandText, Dictionary<string, object> parameters, NpgsqlTransaction transaction)
    {
        var sqlCommand = new NpgsqlCommand(selectCommandText, connection, transaction);
    
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
    
    private static int InsertUser(NpgsqlConnection connection, Record record, NpgsqlTransaction transaction)
    {
        const string selectCommandText = @"SELECT user_id FROM ""user"" WHERE login = @login AND password = @password";
        const string insertCommandText = @"INSERT INTO ""user"" (login, password) VALUES (@login, @password) RETURNING user_id";
        var parameters = new Dictionary<string, object>
        {
            ["login"] = record.UserLogin,
            ["password"] = record.UserPassword
        };
            
        return InsertEntity(connection, selectCommandText, insertCommandText, parameters, transaction);
    }
    
    private static int InsertPlaylist(NpgsqlConnection connection, Record record, int userId, NpgsqlTransaction transaction)
    {
        const string selectCommandText = "SELECT playlist_id FROM playlist WHERE name = @name AND description = @description AND user_id = @user_id";
        const string insertCommandText = "INSERT INTO playlist (name, description, user_id) VALUES (@name, @description, @user_id) RETURNING playlist_id";
        var parameters = new Dictionary<string, object>
        {
            ["name"] = record.PlaylistName,
            ["description"] = record.PlaylistDescription,
            ["user_id"] = userId
        };
    
        return InsertEntity(connection, selectCommandText, insertCommandText, parameters, transaction);
    }
        
    private static int InsertArtist(NpgsqlConnection connection, Record record, NpgsqlTransaction transaction)
    {
        const string selectCommandText = "SELECT artist_id FROM artist WHERE name = @name AND description = @description";
        const string insertCommandText = "INSERT INTO artist (name, description) VALUES (@name, @description) RETURNING artist_id";
        var parameters = new Dictionary<string, object>
        {
            ["name"] = record.ArtistName,
            ["description"] = record.ArtistDescription
        };
    
        return InsertEntity(connection, selectCommandText, insertCommandText, parameters, transaction);
    }
        
    private static int InsertAlbum(NpgsqlConnection connection, Record record, int artistId, NpgsqlTransaction transaction)
    {
        const string selectCommandText = "SELECT album_id FROM album WHERE name = @name AND rating = @rating AND artist_id = @artist_id";
        const string insertCommandText = "INSERT INTO album (name, rating, artist_id) VALUES (@name, @rating, @artist_id) RETURNING album_id";
        var parameters = new Dictionary<string, object>
        {
            ["name"] = record.AlbumName,
            ["rating"] = record.AlbumRating,
            ["artist_id"] = artistId
        };
    
        return InsertEntity(connection, selectCommandText, insertCommandText, parameters, transaction);
    }
        
    private static int InsertSong(NpgsqlConnection connection, Record record, int albumId, NpgsqlTransaction transaction)
    {
        const string selectCommandText = "SELECT song_id FROM song WHERE name = @name AND duration = @duration AND album_id = @album_id";
        const string insertCommandText = "INSERT INTO song (name, duration, album_id) VALUES (@name, @duration, @album_id) RETURNING song_id";
        var parameters = new Dictionary<string, object>
        {
            ["name"] = record.SongName,
            ["duration"] = record.SongDuration,
            ["album_id"] = albumId
        };
    
        return InsertEntity(connection, selectCommandText, insertCommandText, parameters, transaction);
    }
        
    private static void InsertPlaylistSong(NpgsqlConnection connection, Record record, int playlistId, int songId, NpgsqlTransaction transaction)
    {
        const string commandText = "SELECT plays_number FROM playlist_song WHERE playlist_id = @playlist_id AND song_id = @song_id";
        var sqlCommand = new NpgsqlCommand(commandText, connection, transaction);
        sqlCommand.Parameters.AddWithValue("playlist_id", playlistId);
        sqlCommand.Parameters.AddWithValue("song_id", songId);
        var playsNumber = (int?) sqlCommand.ExecuteScalar();
    
        if (playsNumber is null)
        {
            sqlCommand.CommandText = "INSERT INTO playlist_song (playlist_id, song_id, plays_number) VALUES (@playlist_id, @song_id, @plays_number)";
            sqlCommand.Parameters.AddWithValue("plays_number", record.PlaysNumber);
        }
        else
        {
            sqlCommand.CommandText = "UPDATE playlist_song SET plays_number = @plays_number WHERE playlist_id = @playlist_id AND song_id = @song_id";
            sqlCommand.Parameters.AddWithValue("plays_number", playsNumber.Value + record.PlaysNumber);
        }
            
        sqlCommand.ExecuteNonQuery();
    }
}