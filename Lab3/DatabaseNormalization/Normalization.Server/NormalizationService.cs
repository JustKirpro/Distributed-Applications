using System.Threading.Tasks;
using Grpc.Core;
using Normalization.Server.Database;
using Npgsql;

namespace Normalization.Server;

public class NormalizationService : Normalization.NormalizationBase
{
    public override Task<NormalizationReply> Normalize(DatabaseRecord request, ServerCallContext context)
    {
        Record record = new()
        {
            UserLogin = request.UserLogin,
            UserPassword = request.UserPassword,
            PlaylistName = request.PlaylistName, 
            PlaylistDescription = request.PlaylistDescription,
            ArtistName = request.ArtistName,
            ArtistDescription = request.ArtistDescription,
            AlbumName = request.AlbumName,
            AlbumRating = request.AlbumRating,
            SongName = request.SongName,
            SongDuration = request.SongDuration,
            PlaysNumber = request.PlaysNumber
        };
        
        try
        {
            PostgresqlDatabase.InsertRecord(record);
        }
        catch (NpgsqlException)
        {
            return Task.FromResult(new NormalizationReply {IsSuccessful = false});
        }
        
        return Task.FromResult(new NormalizationReply {IsSuccessful = true});
    }
}