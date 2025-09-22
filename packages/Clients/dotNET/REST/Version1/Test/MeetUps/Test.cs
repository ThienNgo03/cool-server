
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Databases.Journal;

namespace Test.MeetUps;

public class Test : BaseTest
{

    #region [ CTors ]

    public Test() : base() { }
    #endregion

    #region [ Endpoints ]

    [Fact]
    public async Task GET()
    {
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var id = Guid.NewGuid();
        var meetUp = new Databases.App.Tables.MeetUp.Table()
        {
            Id = id,
            ParticipantIds = "abc",
            Title = "A basic exercise for upper body strength.",
            DateTime = DateTime.Now,
            CoverImage = "image.png",
            Location = "S10.02",
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        dbContext.MeetUps.Add(meetUp);
        await dbContext.SaveChangesAsync();
        var endpoint = serviceProvider!.GetRequiredService<Library.MeetUps.Interface>();
        var result = await endpoint.AllAsync(new()
        {
            PageIndex = 0,
            PageSize = 10,
        });

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Items);
        dbContext.MeetUps.Remove(meetUp);
        await dbContext.SaveChangesAsync();
    }


    [Fact]

    public async Task POST()
    {
        Guid id = Guid.NewGuid();
        string participantIds = "abc";
        string title = $"A basic exercise for upper body strength. {id}";
        DateTime dateTime = DateTime.Now;
        string location = "S10.02";
        string coverImage = "image.png";

        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        dbContext.MeetUps.RemoveRange(
            dbContext.MeetUps.Where(e => e.ParticipantIds == participantIds && e.Title == title).ToList());
        await dbContext.SaveChangesAsync();
        var meetUpsEndpoint = serviceProvider!.GetRequiredService<Library.MeetUps.Interface>();

        var payload = new Library.MeetUps.Create.Payload
        {
            ParticipantIds = participantIds,
            Title = title,
            DateTime = dateTime,
            Location = location,
            CoverImage = coverImage,
        };
        await meetUpsEndpoint.CreateAsync(payload);

        var expected = await dbContext.MeetUps
            .FirstOrDefaultAsync(e => e.Title == title);
        Assert.NotNull(expected);
        Assert.Equal(title, expected.Title);
        Assert.Equal(participantIds, expected.ParticipantIds);
        Assert.Equal(dateTime, expected.DateTime);
        Assert.Equal(location, expected.Location);
        Assert.Equal(coverImage, expected.CoverImage);

        dbContext.MeetUps.Remove(expected);
        await dbContext.SaveChangesAsync();
    }

    [Fact]

    public async Task PUT()
    {
        var id = Guid.NewGuid();
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var existingMeetUp = new Databases.App.Tables.MeetUp.Table
        {
            Id = id,
            ParticipantIds = "abc",
            Title = "A basic exercise for upper body strength.",
            DateTime = DateTime.Now,
            Location = "S10.02",
            CoverImage = "Image.png",
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        dbContext.MeetUps.Add(existingMeetUp);
        await dbContext.SaveChangesAsync();

        string updatedParticipantIds = "xyz";
        string updatedTitle = "A basic exercise for lower body strength.";
        DateTime updatedDateTime = DateTime.Now;
        string updatedLocation = "S9.02";
        string updatedCoverImage = "image2.png";

        var meetUpsEndpoint = serviceProvider!.GetRequiredService<Library.MeetUps.Interface>();
        var payload = new Library.MeetUps.Update.Payload
        {
            Id = id,
            ParticipantIds = updatedParticipantIds,
            Title = updatedTitle,
            DateTime = updatedDateTime,
            Location = updatedLocation,
            CoverImage = updatedCoverImage,
        };
        await meetUpsEndpoint.UpdateAsync(payload);

        await dbContext.Entry(existingMeetUp).ReloadAsync();
        var updatedMeetUp = existingMeetUp;

        Assert.NotNull(updatedMeetUp);
        Assert.Equal(updatedParticipantIds, updatedMeetUp.ParticipantIds);
        Assert.Equal(updatedTitle, updatedMeetUp.Title);
        Assert.Equal(updatedDateTime, updatedMeetUp.DateTime);
        Assert.Equal(updatedLocation, updatedMeetUp.Location);
        Assert.Equal(updatedCoverImage, updatedMeetUp.CoverImage);

        dbContext.MeetUps.Remove(updatedMeetUp);
        await dbContext.SaveChangesAsync();
    }

    [Fact]

    public async Task DELETE()
    {
        var id = Guid.NewGuid();
        var dbContext = serviceProvider!.GetRequiredService<JournalDbContext>();
        var existingMeetUp = new Databases.App.Tables.MeetUp.Table
        {
            Id = id,
            ParticipantIds = "abc",
            Title = "A basic exercise for upper body strength.",
            DateTime = DateTime.Now,
            Location = "S10.02",
            CoverImage = "Image.png",
            CreatedDate = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        dbContext.MeetUps.Add(existingMeetUp);
        await dbContext.SaveChangesAsync();

        var meetUpsEndpoint = serviceProvider!.GetRequiredService<Library.MeetUps.Interface>();
        await meetUpsEndpoint.DeleteAsync(new Library.MeetUps.Delete.Parameters { Id = id });

        await dbContext.Entry(existingMeetUp).ReloadAsync();
        var deletedMeetUp = await dbContext.Exercises.FindAsync(existingMeetUp.Id);

        Assert.Null(deletedMeetUp);
    }
    #endregion
}
