using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;

namespace Normalization.Client.Database;

public static class SqliteDatabase
{
    private static readonly string ConnectionString = ConfigurationManager.AppSettings.Get("SqliteConnectionString");

    /// <summary>
    /// <exception cref="SQLiteException"> SQLite database problem has occured. </exception>
    /// </summary>
    public static List<Record> ReadRecords()
    {
        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        const string commandText = "SELECT * FROM playlist_song";
        var sqlCommand = new SQLiteCommand(commandText, connection);

        List<Record> records = new();
        using var reader = sqlCommand.ExecuteReader();
        while (reader.Read())
        {
            var record = ReadRecord(reader);
            records.Add(record);
        }

        return records;
    }

    private static Record ReadRecord(IDataRecord reader) =>
        new()
        {
            UserLogin = (string) reader["user_login"],
            UserPassword = (string) reader["user_password"],
            PlaylistName = (string) reader["playlist_name"],
            PlaylistDescription = (string) reader["playlist_description"],
            ArtistName = (string) reader["artist_name"],
            ArtistDescription = (string) reader["artist_description"],
            AlbumName = (string) reader["album_name"],
            AlbumRating = Convert.ToInt32(reader["album_rating"]) != 0 ? Convert.ToInt32(reader["album_rating"]) : null,
            SongName = (string) reader["song_name"],
            SongDuration = Convert.ToInt32(reader["song_duration"]),
            PlaysNumber = Convert.ToInt32(reader["plays_number"])
        };
}