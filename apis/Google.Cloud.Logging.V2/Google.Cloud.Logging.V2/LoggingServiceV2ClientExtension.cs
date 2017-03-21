using Google.Api;
using Google.Api.Gax;
using Google.Api.Gax.Grpc;
using ProtoWellKnownTypes = Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

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
