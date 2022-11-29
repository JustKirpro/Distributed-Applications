using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Normalization.Server.Database;
using Npgsql;

namespace Normalization.Server;

public class NormalizationService : Normalization.NormalizationBase
{
    public override async Task<NormalizationReply> Normalize(IAsyncStreamReader<DatabaseRecord> requestStream, ServerCallContext context)
    {
        List<Record> records = new();
        
        while (await requestStream.MoveNext())
        {
            var databaseRecord = requestStream.Current;
            
            Record record = new()
            {
                UserLogin = databaseRecord.UserLogin,
                UserPassword = databaseRecord.UserPassword,
                PlaylistName = databaseRecord.PlaylistName, 
                PlaylistDescription = databaseRecord.PlaylistDescription,
                ArtistName = databaseRecord.ArtistName,
                ArtistDescription = databaseRecord.ArtistDescription,
                AlbumName = databaseRecord.AlbumName,
                AlbumRating = databaseRecord.AlbumRating,
                SongName = databaseRecord.SongName,
                SongDuration = databaseRecord.SongDuration,
                PlaysNumber = databaseRecord.PlaysNumber
            };

            records.Add(record);
        }
        
        try
        {
            PostgresqlDatabase.InsertRecords(records);
            Console.WriteLine("Records successfully inserted");
            return new NormalizationReply {IsSuccessful = true};
        }
        catch (NpgsqlException)
        {
            return new NormalizationReply {IsSuccessful = false};
        }
    }
}