using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Yarmouk.Helpers;
using Yarmouk.Models;
namespace Yarmouk.Services
{
    public class EvaluationService
    {
        private readonly FirebaseHelper firebase = new FirebaseHelper();
        public Task<Evaluation> GetByIdAsync(string id, string idToken)
        {
            return firebase.GetAsync<Evaluation>(DatabasePaths.Evaluations + "/" + id, idToken);
        }

        public Task SaveAcademicEvaluationAsync(string evaluationId, string placementId, string studentUid,
            string academicSupervisorUid, int score, string feedback, bool submit, string idToken)
        {
            if (string.IsNullOrWhiteSpace(evaluationId) || string.IsNullOrWhiteSpace(placementId)
                || string.IsNullOrWhiteSpace(studentUid) || string.IsNullOrWhiteSpace(academicSupervisorUid)
                || string.IsNullOrWhiteSpace(idToken))
                throw new UnauthorizedAccessException("A verified academic supervisor session is required.");
            if (score < 0 || score > 100) throw new ArgumentOutOfRangeException("score");
            if (feedback == null || feedback.Length > 2000) throw new ArgumentOutOfRangeException("feedback");

            long now = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            var changes = new
            {
                id = evaluationId,
                placementId,
                studentUid,
                academicSupervisorUid,
                academicScore = score,
                academicFeedback = feedback,
                academicSubmitted = submit,
                academicUpdatedAtUnixMs = now,
                academicSubmittedAtUnixMs = submit ? (long?)now : null
            };
            return firebase.UpdateAsync(DatabasePaths.Evaluations + "/" + evaluationId, changes, idToken);
        }
    }
}
