/*
 * Copyright 2017 Google Inc. All Rights Reserved.
 * Use of this source code is governed by a BSD-style
 * license that can be found in the LICENSE file or at
 * https://developers.google.com/open-source/licenses/bsd
 */

using System;
using System.IO;
using System.Linq;
using Google.Protobuf;
using Google.Devtools.Source.V1;

namespace Google.Api
{
    /// <summary>
    /// Read source-context.json from app root.
    /// </summary>
    public static class SourceRevision
    {
        private static Lazy<string> s_sourceContextFilePath = new Lazy<string>(FindSourceContextFile);
        private static Lazy<SourceContext> s_sourceContext = new Lazy<SourceContext>(OpenParseSourceContextFile);
        private static Lazy<string> s_gitRevisionId => new Lazy<string>(() => SourceContextProtoBuf?.Git.RevisionId);


        /// <summary>
        /// The source context file name.
        /// </summary>
        private const string SourceContextFileName = "source-context.json";

        /// <summary>
        /// Gets the source context file path if it exists.
        /// Returns null if the file is not found at root path.
        /// </summary>
        private static string SourceContextFilePath => s_sourceContextFilePath.Value;

        /// <summary>
        /// Returns the <seealso cref="SourceContext"/> object deserealized from the source context file.
        /// Returns null if the file does not exist, or failed to open/parse the file.
        /// </summary>
        private static SourceContext SourceContextProtoBuf => s_sourceContext.Value;

        public const string GitRevisionIdLogLabel = "git_revision_id";
        public static string GitRevisionId => s_gitRevisionId.Value;

        private static SourceContext OpenParseSourceContextFile()
        {
            string sourceContext = ReadSourceContextFile();
            if (sourceContext == null)
            {
                return null;
            }

            try
            {
                return JsonParser.Default.Parse<Devtools.Source.V1.SourceContext>(sourceContext);
            }
            catch (Exception ex) when (IsProtobufParserException(ex))
            {
                return null;
            }
        }

        /// <summary>
        /// Find source context file and open the content.
        /// </summary>
        private static string ReadSourceContextFile()
        {
            try
            {
                using (StreamReader sr = new StreamReader(File.OpenRead(SourceContextFilePath)))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (Exception ex) when (IsIOException(ex))
            {
                return null;
            }
        }

        private static string FindSourceContextFile()
        {
#if NET_CORE
            string root = AppContext.BaseDirectory;
#else
            string root = AppDomain.CurrentDomain.BaseDirectory;
#endif
            var fullPath = Path.Combine(root, SourceContextFileName);
            return File.Exists(fullPath) ? fullPath : null;
        }

        private static bool IsIOException(Exception ex)
        {
            return ex is FileNotFoundException
                || ex is DirectoryNotFoundException
                || ex is IOException;
        }

        private static bool IsProtobufParserException(Exception ex)
        {
            return ex is InvalidProtocolBufferException
                || ex is InvalidJsonException;
        }
    }
}