namespace Normalization.Client.Database;

public readonly struct Record
{
    public string UserLogin { get; init; }
    public string UserPassword { get; init; }
    public string PlaylistName { get; init; }
    public string PlaylistDescription { get; init; }
    public string ArtistName { get; init; }
    public string ArtistDescription { get; init; }
    public string AlbumName { get; init; }
    public int? AlbumRating { get; init; }
    public string SongName { get; init; }
    public int SongDuration { get; init; }
    public int PlaysNumber { get; init; }
}