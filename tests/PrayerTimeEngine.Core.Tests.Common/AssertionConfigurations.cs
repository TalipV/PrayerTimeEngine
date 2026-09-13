using AwesomeAssertions;
using AwesomeAssertions.Equivalency;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

namespace PrayerTimeEngine.Core.Tests.Common;

public static class AssertionConfigurations
{
    private static EquivalencyOptions<T> IEntityContentEquivalency<T>(
        EquivalencyOptions<T> options) where T : IEntity
    {
        return options
            .Excluding(x => x.ID)
            .Excluding(x => x.InsertInstant);
    }

    public static EquivalencyOptions<T> ProfileContentEquivalency<T>(
        EquivalencyOptions<T> options) where T : Profile
    {
        return IEntityContentEquivalency(options)
            .ComparingByMembers<Profile>();
    }

    public static EquivalencyOptions<Profile> MixedProfileContentEquivalency(
        EquivalencyOptions<Profile> options)
    {
        return ProfileContentEquivalency(options)
            .Using<DynamicProfile>(ctx =>
                ctx.Subject.Should().BeEquivalentTo(
                    ctx.Expectation, o => DynamicProfileContentEquivalency(o)))
            .When(info => info.RuntimeType == typeof(DynamicProfile))
            .Using<MosqueProfile>(ctx =>
                ctx.Subject.Should().BeEquivalentTo(
                    ctx.Expectation, o => MosqueProfileContentEquivalency(o)))
            .When(info => info.RuntimeType == typeof(MosqueProfile));
    }

    public static EquivalencyOptions<MosqueProfile> MosqueProfileContentEquivalency(
        EquivalencyOptions<MosqueProfile> options)
    {
        return ProfileContentEquivalency(options);
    }

    public static EquivalencyOptions<DynamicProfile> DynamicProfileContentEquivalency(
        EquivalencyOptions<DynamicProfile> options)
    {
        return ProfileContentEquivalency(options)
            .ComparingByMembers<ProfilePlaceInfo>()
            .ComparingByMembers<TimezoneInfo>()
            .ComparingByMembers<ProfileLocationConfig>()
            .ComparingByMembers<ProfileTimeConfig>()
            .Excluding(x => x.PlaceInfo.ID)
            .Excluding(x => x.PlaceInfo.ProfileID)
            .Excluding(x => x.PlaceInfo.Profile)
            .Excluding(x => x.PlaceInfo.InsertInstant)
            .Excluding(x => x.PlaceInfo.TimezoneInfo.ID)
            .Excluding(x => x.PlaceInfo.TimezoneInfo.InsertInstant)
            .For(x => x.LocationConfigs).Exclude(x => x.ID)
            .For(x => x.LocationConfigs).Exclude(x => x.ProfileID)
            .For(x => x.LocationConfigs).Exclude(x => x.Profile)
            .For(x => x.LocationConfigs).Exclude(x => x.InsertInstant)
            .For(x => x.TimeConfigs).Exclude(x => x.ID)
            .For(x => x.TimeConfigs).Exclude(x => x.ProfileID)
            .For(x => x.TimeConfigs).Exclude(x => x.Profile)
            .For(x => x.TimeConfigs).Exclude(x => x.InsertInstant);
    }
}
