using Google.Api.Gax.Grpc;

namespace Google.Cloud.Logging.V2
{
    public sealed partial class LoggingServiceV2ClientImpl : LoggingServiceV2Client
    {
        private const string SourceContextIDLabel = "source_context_id";
        private const string SecondarySourceContextIDLabel = "gcloud_source_context_id";

        partial void Modify_WriteLogEntriesRequest(ref WriteLogEntriesRequest request, ref CallSettings settings)
        {
            var gitSha = SourceContext.Current?.GitSha;
            if (gitSha == null)
            {
                return;
            }

            foreach (var log in request.Entries)
            {
                if (!log.Labels.ContainsKey(SourceContextIDLabel))
                {
                    log.Labels[SourceContextIDLabel] = gitSha;
                }
                else if (!log.Labels.ContainsKey(SecondarySourceContextIDLabel))
                {
                    log.Labels[SecondarySourceContextIDLabel] = gitSha;
                }
            }
        }
    }
}
