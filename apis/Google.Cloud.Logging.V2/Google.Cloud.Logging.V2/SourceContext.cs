/*
 * Copyright 2017 Google Inc. All Rights Reserved.
 * Use of this source code is governed by a BSD-style
 * license that can be found in the LICENSE file or at
 * https://developers.google.com/open-source/licenses/bsd
 */

using System;
using System.IO;
using Google.Protobuf;
using ProtoWellKnownTypes = Google.Protobuf.WellKnownTypes;

namespace Google.Cloud.Logging.V2
{
    /// <summary>
    /// Read source-context.json from app domain root.
    /// </summary>
    public class SourceContext
    {
        private const string SourceContextFileName = "source-context.json";
        private ProtoWellKnownTypes.Struct _sourceContext => FindSourceContext();
        private ProtoWellKnownTypes.Struct _git => _sourceContext?.Fields["git"]?.StructValue;
        private readonly static Lazy<SourceContext> s_instance = new Lazy<SourceContext>(() => new SourceContext());

        /// <summary>
        /// Best way to get application folder path.
        /// http://stackoverflow.com/questions/6041332/best-way-to-get-application-folder-path
        /// </summary>
        private static string SourceContextFilePath {
            get
            {
                string root = null;
#if NET_CORE
                var rootAssembly = System.Reflection.Assembly.GetEntryAssembly();
                root = rootAssembly?.Location;
#else
                root = System.AppDomain.CurrentDomain.BaseDirectory;
#endif
                return Path.Combine(root, SourceContextFileName);
            }
        }
            //Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SourceContextFileName);

        private SourceContext() { }

        /// <summary>
        /// The git SHA from Source Context file.
        /// </summary>
        public string GitSha => _git?.Fields["revisionId"].StringValue;

        /// <summary>
        /// The git repo URL from Source Context file.
        /// </summary>
        public string GitRepoUrl => _sourceContext?.Fields["url"].StringValue;

        /// <summary>
        /// The singleton instance of the <seealso cref="SourceContext"/> class.
        /// </summary>
        public static SourceContext Current => s_instance.Value;

        private static ProtoWellKnownTypes.Struct FindSourceContext()
        {
            string sourceContext = ReadSourceContextFile();
            if (sourceContext == null)
            {
                return null;
            }
            try
            {
                return JsonParser.Default.Parse<ProtoWellKnownTypes.Struct>(sourceContext);
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