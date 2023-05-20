using System.Linq.Expressions;
using Core.Entities;

namespace Core.Specifications.Media;

public class MediaAssetSpec : Specification<MediaAsset>
{
    public MediaAssetSpec(int id, string userId) : base(md => md.Id == id && md.UserId == userId)
    {
    }

    public MediaAssetSpec(int mediaId, int trainingLogId) : base(m => m.Id == mediaId && m.TrainingLogId == trainingLogId)
    {
    }
}